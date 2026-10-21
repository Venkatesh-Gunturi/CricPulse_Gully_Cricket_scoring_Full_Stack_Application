namespace CricPulse.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> SaveProfileImageAsync(
        Stream fileStream,
        string fileName,
        string contentType);

    Task<string> SaveMatchLogoAsync(
        Stream fileStream,
        string fileName,
        string contentType);

    Task DeleteAsync(string? relativePath);
}