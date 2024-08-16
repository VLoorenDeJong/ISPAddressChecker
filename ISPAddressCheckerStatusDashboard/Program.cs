using ISPAddressChecker.Interfaces;
using Microsoft.Extensions.Options;
using ISPAddressChecker.Options;
using ISPAddressChecker.Models.Constants;
using ISPAddressCheckerDashboard.Services;
using ISPAddressCheckerStatusDashboard;
using ISPAddressCheckerStatusDashboard.Services;

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



builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();

// Add services for controllers
builder.Services.AddControllers();

builder.Services.Configure<DashboardApplicationSettingsOptions>(builder.Configuration.GetSection(AppsettingsSections.ApplicationSettings));
builder.Services.Configure<EmailSettingsOptions>(builder.Configuration.GetSection(AppsettingsSections.EmailSettings));

builder.Services.AddSingleton<ICounterService, CounterService>();

builder.Services.AddScoped<IDashboardTimerService, TimerService>();
builder.Services.AddScoped<IStatusService, StatusService>();

builder.Services.AddTransient<IOpenAPIClient, OpenAPIClient>();
builder.Services.AddTransient<IRequestEmailService, RequestEmailService>();
builder.Services.AddTransient<IRequestISPAddressService, RequestISPAddressService>();
builder.Services.AddTransient<IDashboardConfigCheckService, ApplicationConfigCheckService>();
builder.Services.AddTransient<IISPAddressCheckerStatusService, ISPAddressCheckerStatusService>();
builder.Services.AddTransient<IDashboardEmailService, EmailService>();
builder.Services.AddTransient<IHTTPClientControllerMessageService, HTTPClientControllerMessageService>();

var app = builder.Build();

// Checking the configuration of the application:
// Get the application settings from the service provider
var applicationSettingsOptions = app.Services.GetRequiredService<IOptions<DashboardApplicationSettingsOptions>>();
var emailSettingsOptions = app.Services.GetRequiredService<IOptions<EmailSettingsOptions>>();

// Get the service from the service provider
var applicationConfigCheckService = app.Services.GetRequiredService<IDashboardConfigCheckService>();

// Call the method to check the application settings
await applicationConfigCheckService.CheckApplicationConfig(applicationSettingsOptions, emailSettingsOptions);


if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.MapControllers(); // Map API controllers
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
