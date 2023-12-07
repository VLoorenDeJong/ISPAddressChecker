using ISPAddressChecker.Options;
using ISPAddressChecker.Services;
using ISPAddressChecker.Services.Interfaces;
using ISPAddressChecker.SignalRHubs;
using ISPAddressChecker.SignalRHubs.Interfaces;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.SignalR;
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
builder.Services.AddSingleton<IActionContextAccessor,  ActionContextAccessor>();

builder.Services.AddTransient<ICheckISPAddressService, CheckISPAddressService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<ILogHubService, LogHubService>();

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

// ToDo Add uptime to the heartbeat email
// ToDo: Switch on hubs only when dashboard is active
app.MapHub<ClockHub>(SignalRHubUrls.ClockHubURL);
app.MapHub<LogHub>(SignalRHubUrls.LogHubURL);
app.MapControllers();
app.Run();