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


builder.Services.AddSingleton<ApplicationState>();
builder.Services.AddScoped<SeasonSideEffects>();
builder.Services.AddScoped<UserSideEffects>();
builder.Services.AddScoped<LocalStorageSideEffects>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddPWAUpdater();
builder.Services.RegisterHttpServices(builder.Configuration.GetValue<string>("ApiUrl") ?? builder.HostEnvironment.BaseAddress);
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
    config.SnackbarConfiguration.VisibleStateDuration = 8000;
});

builder.Services.AddStaticWebAppsAuthentication();

await builder.Build().RunAsync();