using static ISPAddressCheckerStatusDashboard.Options.ApplicationSettingsOptions;
using ISPAddressCheckerStatusDashboard.Services.Interfaces;
using ISPAddressCheckerStatusDashboard.Services;
using ISPAddressCheckerStatusDashboard.Options;
using ISPAddressCheckerStatusDashboard;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<ClockHubClient>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.Configure<ApplicationSettingsOptions>(builder.Configuration.GetSection(AppsettingsSections.ApplicationSettings));

builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddSingleton<ICounterService, CounterService>();

builder.Services.AddScoped<ITimerService, TimerService>();

builder.Services.AddTransient<IOpenAPIClient, OpenAPIClient>();
builder.Services.AddTransient<IRequestEmailService, RequestEmailService>();
builder.Services.AddTransient<IRequestISPAddressService, RequestISPAddressService>();
builder.Services.AddTransient<IISPAddressCheckerStatusService, ISPAddressCheckerStatusService>();

//ToDo Email text box input changed after sending. Sends email to old address? (Email adress not updated after input second email address)
// ToDo Enpoint URL removal from appsettings

var app = builder.Build();

//Start the application
//app.Services.GetService<IApplicationService>()!.StartAsync(default).GetAwaiter().GetResult();

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
