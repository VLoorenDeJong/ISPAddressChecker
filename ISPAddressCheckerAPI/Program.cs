using ISPAddressChecker.Interfaces;
using ISPAddressChecker.Models.Constants;
using ISPAddressChecker.Options;
using ISPAddressChecker.Services;
using ISPAddressChecker.SignalRHubs;
using ISPAddressCheckerAPI.Services;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

string environment = builder.Environment.EnvironmentName;
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Program.cs -> Environment: {environment}");
Console.ResetColor();

// Add environment-specific configuration files
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables(); // Add environment variables

#if DEBUG
// Log only appsettings values
foreach (var kvp in builder.Configuration.AsEnumerable())
{
    if (kvp.Key.StartsWith(AppsettingsSections.ApplicationSettings) || kvp.Key.StartsWith(AppsettingsSections.EmailSettings))
    {
        Console.WriteLine($"{kvp.Key}: {kvp.Value}");
    }
}
#endif

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHostedService<ClockWorker>();
//builder.Services.AddHostedService<LogWorker>();

// Add HttpClient and CheckISPAddressService
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IISPAddressService, ISPAddressService>();
builder.Services.AddSingleton<ITimerService, TimerService>();
builder.Services.AddSingleton<IISPAddressCounterService, ISPAddressCounterService>();
builder.Services.AddSingleton<IStatusCounterService, StatusCounterService>();
builder.Services.AddSingleton<IApplicationService, ApplicationService>();
builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

builder.Services.AddTransient<ICheckISPAddressService, CheckISPAddressService>();
builder.Services.AddTransient<IAPIEmailService, EmailService>();
builder.Services.AddTransient<ILogHubService, LogHubService>();
builder.Services.AddTransient<IAPIConfigCheckService, ApplicationConfigCheckService>();

builder.Services.Configure<APIApplicationSettingsOptions>(builder.Configuration.GetSection(AppsettingsSections.ApplicationSettings));
builder.Services.Configure<APIEmailSettingsOptions>(builder.Configuration.GetSection(AppsettingsSections.EmailSettings));

var app = builder.Build();

//Start the application
app.Services.GetService<IApplicationService>()!.StartAsync(default).GetAwaiter().GetResult();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthorization();

// Checking the configuration of the application:
// Get the application settings from the service provider
var applicationSettingsOptions = app.Services.GetRequiredService<IOptions<APIApplicationSettingsOptions>>();

// Get the service from the service provider
var applicationConfigCheckService = app.Services.GetRequiredService<IAPIConfigCheckService>();

// Call the method to check the application settings
applicationConfigCheckService.CheckApplicationConfig(applicationSettingsOptions);

bool dashboardActive = applicationSettingsOptions.Value.DashboardEnabled;

if (dashboardActive)
{
    app.MapHub<ClockHub>(SignalRHubUrls.ClockHubURL);
    app.MapHub<LogHub>(SignalRHubUrls.LogHubURL);
}
app.MapControllers();
app.Run();


// ToDo: Change request devision to failed request instead of external request of the API endpoint
// ToDo: Add the time of the next check into the health indicator
// ToDo: Check the functionality with real world test
// ToDo: Add protection rules on branch (Pull request mendatory)
// ToDo: Create WIKI entry for the project
// ToDo: Check website with VPN for user ISP / User ISP address is not displayed
// ToDo: Font size need to be bigger for phones
// ToDo: Change color sceme
