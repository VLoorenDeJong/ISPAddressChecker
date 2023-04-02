using static ISPAdressCheckerStatusDashboard.Options.ApplicationSettingsOptions;
using ISPAdressCheckerStatusDashboard.Services.Interfaces;
using ISPAdressCheckerStatusDashboard.Services;
using ISPAdressCheckerStatusDashboard.Options;
using ISPAdressCheckerStatusDashboard;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.Configure<ApplicationSettingsOptions>(builder.Configuration.GetSection(AppsettingsSections.ApplicationSettings));

builder.Services.AddSingleton<IApplicationService, ApplicationService>();

builder.Services.AddScoped<ITimerService, TimerService>();

builder.Services.AddTransient<IISPAddressCheckerStatusService, ISPAddressCheckerStatusService>();
builder.Services.AddTransient<IOpenAPIClient, OpenAPIClient>();
builder.Services.AddTransient<IISPAddressCheckerStatusService, ISPAddressCheckerStatusService>();


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
