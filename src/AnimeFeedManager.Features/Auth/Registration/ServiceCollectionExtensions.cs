using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Passwordless;

namespace AnimeFeedManager.Features.Auth.Registration;

public static class ServiceCollectionExtensions
{
    private const string PasswordlessSection = "Passwordless";

    extension(IHostApplicationBuilder builder)
    {
        /// <summary>
        /// Registers the Auth feature's external integration: the Passwordless SDK
        /// (<see cref="IPasswordlessClient"/>) bound from the <c>"Passwordless"</c> configuration
        /// section. <see cref="PasswordlessOptions"/> is also registered so the host can read the
        /// public API key (for the browser client). Mirrors how the Library feature owns its Jikan
        /// client registration.
        ///
        /// Domain flows (registration/login) take capability delegates constructed at the call site,
        /// so they are not registered here. The cookie auth scheme, <c>admin_required</c> policy, and
        /// browser client assets are wired in the Web host.
        /// </summary>
        public IHostApplicationBuilder AddAuth()
        {
            builder.Services.Configure<PasswordlessOptions>(
                builder.Configuration.GetSection(PasswordlessSection));
            builder.Services.AddPasswordlessSdk(options =>
                builder.Configuration.GetRequiredSection(PasswordlessSection).Bind(options));
            return builder;
        }
    }
}
