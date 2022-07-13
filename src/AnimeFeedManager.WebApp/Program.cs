using AnimeFeedManager.WebApp;
using AnimeFeedManager.WebApp.Services;
using AnimeFeedManager.WebApp.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Azure.Functions.Authentication.WebAssembly;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddSingleton<ApplicationState>();
var baseApiUri = builder.Configuration.GetValue<string>("ApiUrl") ?? builder.HostEnvironment.BaseAddress;

builder.Services.AddHttpClient<ISeasonFetcherService, SeasonService>(client =>
    client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp,_) => HttpClientPolicies.GetRetryPolicy(sp));

builder.Services.AddHttpClient<ISeasonCollectionFetcher, SeasonCollectionService>(client =>
    client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));

builder.Services.AddHttpClient<IUserService, UserService>(client =>
    client.BaseAddress = new Uri($"{baseApiUri}")).AddPolicyHandler((sp, _) => HttpClientPolicies.GetRetryPolicy(sp));

builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;
});

builder.Services.AddStaticWebAppsAuthentication();

await builder.Build().RunAsync();
