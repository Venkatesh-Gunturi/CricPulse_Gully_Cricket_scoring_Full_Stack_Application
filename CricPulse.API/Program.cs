using CricPulse.API.Background;
using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;
using CricPulse.Application.Services.Auth;
using CricPulse.Application.Services.Match;
using CricPulse.Application.Services.Player;
using CricPulse.Application.Services.Scoring;
using CricPulse.Infrastructure.Data;
using CricPulse.Infrastructure.Repositories;
using CricPulse.Infrastructure.Security;
using CricPulse.Infrastructure.Services.Location;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<CricPulseDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IUserAccountProfileService, UserAccountProfileService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPlayerStatisticsRepository, PlayerStatisticsRepository>();
builder.Services.AddScoped<PlayerLookupService>();
builder.Services.AddScoped<IOrganizerStatisticsRepository, OrganizerStatisticsRepository>();
builder.Services.AddScoped<IMatchRepository, MatchRepository>();
builder.Services.AddScoped<IInningsRepository, InningsRepository>();
builder.Services.AddScoped<IBallRepository, BallRepository>();
builder.Services.AddScoped<IWicketRepository, WicketRepository>();
builder.Services.AddScoped<IMatchScoringRepository, MatchScoringRepository>();
builder.Services.AddScoped<ITransactionManager, TransactionManager>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IForgotPasswordService, ForgotPasswordService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IScoringService, ScoringService>();
builder.Services.AddScoped<IMatchCompletionService, MatchCompletionService>();
builder.Services.AddScoped<IMatchLifecycleService, MatchLifecycleService>();

builder.Services.AddHostedService<MatchLifecycleWorker>();

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<
    IReverseGeocodingService,
    NominatimReverseGeocodingService>(client =>
    {
        client.BaseAddress =
            new Uri("https://nominatim.openstreetmap.org/");

        client.Timeout = TimeSpan.FromSeconds(10);

        var userAgent = builder.Configuration[
            "Geocoding:UserAgent"];

        if (string.IsNullOrWhiteSpace(userAgent) ||
            userAgent.Contains("replace-with", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Configure Geocoding:UserAgent with an identifiable CricPulse application name and contact before starting the API.");
        }

        client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
    });

//CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowCricPulseFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowCricPulseFrontend");





app.MapControllers();

app.Run();