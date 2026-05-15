using AnimeFeedManager.Features.Library.Import.Jikan.Registration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WireMock.Server;

namespace AnimeFeedManager.Features.Tests.Library.Import.Jikan.Helpers;

/// <summary>
/// Spins up a WireMock server + a real <see cref="IJikanClient"/> wired through the
/// production resilience pipeline. Tests configure stubs on <see cref="Server"/> and
/// drive <see cref="Client"/>. Retry delay is shortened so retry-exhaustion paths
/// complete in milliseconds rather than seconds.
/// </summary>
internal sealed class JikanTestHost : IAsyncDisposable
{
    public WireMockServer Server { get; }
    public IJikanClient Client { get; }
    private readonly IHost _host;

    private JikanTestHost(WireMockServer server, IHost host, IJikanClient client)
    {
        Server = server;
        _host = host;
        Client = client;
    }

    public static JikanTestHost Create(int maxRetryAttempts = 3, TimeSpan? retryBaseDelay = null)
    {
        var server = WireMockServer.Start();

        var builder = Host.CreateEmptyApplicationBuilder(null);
        builder.AddJikanClient();
        builder.Services.Configure<JikanOptions>(opts =>
        {
            opts.BaseUrl = server.Url + "/";
            opts.RequestTimeout = TimeSpan.FromSeconds(10);
            opts.MaxRetryAttempts = maxRetryAttempts;
            opts.RetryBaseDelay = retryBaseDelay ?? TimeSpan.FromMilliseconds(10);
        });

        var host = builder.Build();
        var client = host.Services.GetRequiredService<IJikanClient>();
        return new JikanTestHost(server, host, client);
    }

    public ValueTask DisposeAsync()
    {
        _host.Dispose();
        Server.Stop();
        Server.Dispose();
        return ValueTask.CompletedTask;
    }

    public static string LoadFixture(string fileName) =>
        File.ReadAllText(Path.Combine("Library", "Import", "Jikan", "Fixtures", fileName));
}
