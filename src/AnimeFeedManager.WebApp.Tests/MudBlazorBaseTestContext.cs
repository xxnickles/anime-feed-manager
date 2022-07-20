using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;

namespace AnimeFeedManager.WebApp.Tests;

public class MudBlazorBaseTestContext : TestContext
{
    protected MudBlazorBaseTestContext()
    {
        Services.AddSingleton(A.Dummy<IKeyInterceptor>());
        Services.AddSingleton(A.Dummy<IScrollManager>());
        Services.AddSingleton(A.Dummy<IMudPopoverService>());
        Services.AddSingleton(A.Dummy<IJsApiService>());
    }
}