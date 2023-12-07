using static ISPAddressCheckerStatusDashboard.Options.ApplicationSettingsOptions;
using ISPAddressCheckerStatusDashboard.Services.Interfaces;
using ISPAddressCheckerStatusDashboard.Services;
using ISPAddressCheckerStatusDashboard.Options;
using ISPAddressCheckerStatusDashboard;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ClockHubClient>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.Configure<ApplicationSettingsOptions>(builder.Configuration.GetSection(AppsettingsSections.ApplicationSettings));
builder.Services.Configure<EmailSettingsOptions>(builder.Configuration.GetSection(AppsettingsSections.EmailSettings));

builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddSingleton<ICounterService, CounterService>();

builder.Services.AddScoped<ITimerService, TimerService>();

builder.Services.AddTransient<IOpenAPIClient, OpenAPIClient>();
builder.Services.AddTransient<IRequestEmailService, RequestEmailService>();
builder.Services.AddTransient<IRequestISPAddressService, RequestISPAddressService>();
builder.Services.AddTransient<IApplicationConfigCheckService, ApplicationConfigCheckService>();
builder.Services.AddTransient<IISPAddressCheckerStatusService, ISPAddressCheckerStatusService>();
builder.Services.AddTransient<ISPAddressCheckerStatusDashboard.Services.Interfaces.IEmailService, ISPAddressCheckerStatusDashboard.Services.EmailService>();


//ToDo Email text box input changed after sending. Sends email to old address? (Email adress not updated after input second email address)
// ToDo Enpoint URL removal from appsettings

var app = builder.Build();


// Checking the configuration of the application:
    // Get the application settings from the service provider
    var applicationSettingsOptions = app.Services.GetRequiredService<IOptions<ApplicationSettingsOptions>>();
    var emailSettingsOptions = app.Services.GetRequiredService<IOptions<EmailSettingsOptions>>();
// Get the service from the service provider
var applicationConfigCheckService = app.Services.GetRequiredService<IApplicationConfigCheckService>();
    // Call the method to check the application settings
    applicationConfigCheckService.CheckApplicationConfig(applicationSettingsOptions, emailSettingsOptions);

// ToDo: Add emailconfig checks
// ToDo: Add send applicationRunning 


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
