using AnimeFeedManager.WebApp;
using AnimeFeedManager.WebApp.State;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Azure.Functions.Authentication.WebAssembly;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddSingleton<ApplicationState>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.RegisterHttpServices(builder.Configuration.GetValue<string>("ApiUrl") ??
                                      builder.HostEnvironment.BaseAddress);
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
});

builder.Services.AddStaticWebAppsAuthentication();

await builder.Build().RunAsync();