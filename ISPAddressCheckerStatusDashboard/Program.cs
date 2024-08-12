using ISPAddressChecker.Interfaces;
using Microsoft.Extensions.Options;
using ISPAddressChecker.Options;
using ISPAddressChecker.Models.Constants;
using ISPAddressCheckerDashboard.Services;
using ISPAddressCheckerStatusDashboard;
using ISPAddressCheckerStatusDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

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

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
