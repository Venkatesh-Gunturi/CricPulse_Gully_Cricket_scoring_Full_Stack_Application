using CricPulse.API.Middleware;
using CricPulse.Application.Interfaces;
using CricPulse.Application.Interfaces.Auth;
using CricPulse.Application.Interfaces.Location;
using CricPulse.Application.Interfaces.Match;
using CricPulse.Application.Interfaces.Otp;
using CricPulse.Application.Interfaces.player;
using CricPulse.Application.Interfaces.Player;
using CricPulse.Application.Interfaces.User;
using CricPulse.Application.Services;
using CricPulse.Application.Services.Auth;
using CricPulse.Application.Services.Match;
using CricPulse.Application.Services.Player;
using CricPulse.Application.Services.User;
using CricPulse.Infrastructure.Authentication;
using CricPulse.Infrastructure.Data;
using CricPulse.Infrastructure.Repositories;
using CricPulse.Infrastructure.Repositories.Match;
using CricPulse.Infrastructure.Services.Location;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IOtpRepository, OtpRepository>();

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();

builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IMatchScoringService, MatchScoringService>();
builder.Services.AddScoped<IScoringRepository, ScoringRepository>();
builder.Services.AddScoped<IPendingRegistrationRepository, PendingRegistrationRepository>();

//External location service to get the match state
builder.Services.AddHttpClient<ILocationService, LocationService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "CricPulse/1.0");
});


//JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt");

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],

            IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["key"]!))
        };
    });
builder.Services.AddControllers();

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter JWT token"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddDbContext<CricPulseDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


var app = builder.Build();

//Enabling the CORS policy
app.UseCors("ReactPolicy");

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
