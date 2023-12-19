using ISPAddressChecker.Options;
using ISPAddressChecker.Services;
using ISPAddressChecker.Services.Interfaces;
using ISPAddressChecker.SignalRHubs;
using ISPAddressChecker.SignalRHubs.Interfaces;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using MyApplication;
using static ISPAddressChecker.Models.Enums.Constants;
using static ISPAddressChecker.Options.ApplicationSettingsOptions;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<ILogHubService, LogHubService>();
builder.Services.AddTransient<IApplicationConfigCheckService, ApplicationConfigCheckService>();

builder.Services.Configure<ApplicationSettingsOptions>(builder.Configuration.GetSection(AppsettingsSections.ApplicationSettings));
builder.Services.Configure<EmailSettingsOptions>(builder.Configuration.GetSection(AppsettingsSections.EmailSettings));

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

// ToDo: Add uptime to the heartbeat email
// ToDo: add email requested counter
// ToDo: add email requested counter to the heartbeat email



// Checking the configuration of the application:
// Get the application settings from the service provider
var applicationSettingsOptions = app.Services.GetRequiredService<IOptions<ApplicationSettingsOptions>>();

// Get the service from the service provider
var applicationConfigCheckService = app.Services.GetRequiredService<IApplicationConfigCheckService>();

// Call the method to check the application settings
applicationConfigCheckService.CheckApplicationConfig(applicationSettingsOptions);

bool dashboardActive = applicationSettingsOptions.Value.DashboardEnabled;
                     ;
if (dashboardActive)
{
    app.MapHub<ClockHub>(SignalRHubUrls.ClockHubURL);
    app.MapHub<LogHub>(SignalRHubUrls.LogHubURL);
}
app.MapControllers();
app.Run();

