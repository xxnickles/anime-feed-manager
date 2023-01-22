using System.Reflection;
using AnimeFeedManager.WebApp;
using AnimeFeedManager.WebApp.Authentication;
using AnimeFeedManager.WebApp.State;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(new Metadata(Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "fail"));
builder.Services.AddSingleton<ApplicationState>();
builder.Services.AddScoped<SeasonSideEffects>();
builder.Services.AddScoped<UserSideEffects>();
builder.Services.AddScoped<LocalStorageSideEffects>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddPWAUpdater();
builder.Services.RegisterHttpServices(builder.Configuration.GetValue<string>("ApiUrl") ?? builder.HostEnvironment.BaseAddress);
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PreventDuplicates = true;
    config.SnackbarConfiguration.MaxDisplayedSnackbars = 1;
    config.SnackbarConfiguration.BackgroundBlurred = true;
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.VisibleStateDuration = 6000;
});

builder.Services.AddStaticWebAppsAuthentication();

await builder.Build().RunAsync();