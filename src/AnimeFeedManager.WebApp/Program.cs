using AnimeFeedManager.WebApp;
using AnimeFeedManager.WebApp.Services;
using AnimeFeedManager.WebApp.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddSingleton<ApplicationState>();
var baseApiUri = builder.Configuration.GetValue<string>("ApiUrl") ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddHttpClient<ISeasonFetcherService, SeasonFetcherService>(client =>
    client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp,_) => HttpClientPolicies.GetRetryPolicy(sp));

builder.Services.AddHttpClient<ISeasonCollectionFetcher, SeasonCollectionFetcher>(client =>
    client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
});


await builder.Build().RunAsync();
