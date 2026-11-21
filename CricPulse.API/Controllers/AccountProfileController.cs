
using CricPulse.Application.DTOs.Player;
using CricPulse.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace CricPulse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountProfileController : ControllerBase
{
    private readonly IUserAccountProfileService _profileService;
    private const int MaxPhotoBytes = 2 * 1024 * 1024;
    private static readonly JsonSerializerOptions ProfileJsonOptions =
        new(JsonSerializerDefaults.Web);
    private readonly string _photoDirectory;

    public AccountProfileController(
    IUserAccountProfileService profileService,
    IWebHostEnvironment environment,
    IConfiguration configuration)
    {
        _profileService = profileService;
        _photoDirectory = configuration["ProfilePhotos:Directory"]
            ?? Path.Combine(environment.ContentRootPath, "profile-photos");
    }

    [Authorize]
    [HttpGet("{userId:guid}")]
    public async Task<IActionResult> GetProfile(Guid userId)
    {
        if (!IsProfileOwner(userId))
            return Forbid();

        try
        {
            return Ok(await _profileService.GetProfileAsync(userId));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("public/{userId:guid}")]
    public async Task<IActionResult> GetPublicProfile(Guid userId)
    {
        try
        {
            var result = await _profileService.GetPublicProfileAsync(userId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("{userId:guid}")]
    [Consumes("application/json")]
    public Task<IActionResult> UpdateProfile(
    Guid userId, [FromBody] UpdateUserProfileDto dto)
    {
        dto.ProfileImageUrl = null;
        return SaveProfileAsync(userId, dto);
    }

    [Authorize]
    [HttpPut("{userId:guid}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxPhotoBytes + 64 * 1024)]
    public async Task<IActionResult> UploadProfilePhoto(
    Guid userId, [FromForm] string profile, [FromForm] IFormFile photo)
    {
        if (!IsProfileOwner(userId))
            return Forbid();

        UpdateUserProfileDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<UpdateUserProfileDto>(
                profile, ProfileJsonOptions);
        }
        catch (JsonException)
        {
            return BadRequest(new { message = "Invalid profile details." });
        }

        if (dto is null || photo.Length < 8 || photo.Length > MaxPhotoBytes)
            return BadRequest(new { message = "Choose a JPEG or PNG photo up to 2 MB." });

        await using var input = photo.OpenReadStream();
        var header = new byte[8];
        await input.ReadExactlyAsync(header, HttpContext.RequestAborted);

        var extension = header switch
        {
            [0xff, 0xd8, 0xff, ..] => "jpg",
            [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a] => "png",
            _ => null
        };

        if (extension is null)
            return BadRequest(new { message = "Choose a JPEG or PNG photo." });

        var fileName = $"{Guid.NewGuid():N}.{extension}";
        Directory.CreateDirectory(_photoDirectory);
        var path = Path.Combine(_photoDirectory, fileName);

        try
        {
            await using (var output = System.IO.File.Create(path))
            {
                await output.WriteAsync(header, HttpContext.RequestAborted);
                await input.CopyToAsync(output, HttpContext.RequestAborted);
            }

            dto.ProfileImageUrl = $"/api/AccountProfile/photos/{fileName}";
            var result = await SaveProfileAsync(userId, dto);

            if (result is OkObjectResult)
                return result;

            System.IO.File.Delete(path);
            return result;
        }
        catch
        {
            System.IO.File.Delete(path);
            throw;
        }
    }

    [AllowAnonymous]
    [HttpGet("photos/{fileName}")]
    public IActionResult GetPhoto(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (extension is not (".jpg" or ".png") ||
            !Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _) ||
            fileName != Path.GetFileName(fileName))
            return NotFound();

        var path = Path.GetFullPath(Path.Combine(_photoDirectory, fileName));
        if (!System.IO.File.Exists(path))
            return NotFound();

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(path, extension == ".png" ? "image/png" : "image/jpeg");
    }

    private async Task<IActionResult> SaveProfileAsync(
    Guid userId, UpdateUserProfileDto dto)
    {
        if (!IsProfileOwner(userId))
            return Forbid();
        try
        {
            var result = await _profileService.UpdateProfileAsync(
                userId, dto);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private bool IsProfileOwner(Guid userId) =>
    Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var callerId)
    && callerId == userId;
}
