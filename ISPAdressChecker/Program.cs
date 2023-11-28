using ISPAdressChecker.Options;
using ISPAdressChecker.Services;
using ISPAdressChecker.Services.Interfaces;
using ISPAdressChecker.SignalRHubs;
using ISPAdressChecker.SignalRHubs.Interfaces;
using Microsoft.AspNetCore.SignalR;
using MyApplication;
using static ISPAdressChecker.Options.ApplicationSettingsOptions;

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
builder.Services.AddSingleton<IISPAdressCounterService, ISPAdressCounterService>();
builder.Services.AddSingleton<IStatusCounterService, StatusCounterService>();
builder.Services.AddSingleton<IApplicationService, ApplicationService>();

builder.Services.AddTransient<ICheckISPAddressService, CheckISPAddressService>();
builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.Configure<ApplicationSettingsOptions>(builder.Configuration.GetSection(AppsettingsSections.ApplicationSettings));

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

// ToDo make loghub URL configurable via appsettings.json
// ToDo: make the url available via a endpoint
app.MapHub<ClockHub>("/hubs/clock");
app.MapHub<LogHub>("/hubs/log");
app.MapControllers();
app.Run();