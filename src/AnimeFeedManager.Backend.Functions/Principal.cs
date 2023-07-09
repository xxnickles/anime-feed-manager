using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AnimeFeedManager.Backend.Functions;

public class ClientPrincipal
{
    public string? IdentityProvider { get; set; }
    public string? UserId { get; set; }
    public string? UserDetails { get; set; }
    public IEnumerable<string>? UserRoles { get; set; }

    public static Task<ClaimsPrincipal> ParseFromRequest(HttpRequestData request)
    {
        var principal = new ClientPrincipal();

        if (request.Headers.TryGetValues("x-ms-client-principal", out var values))
        {
            var data = values.First();
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);
            principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        principal!.UserRoles = principal.UserRoles?.Except(new[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

        if (!principal?.UserRoles?.Any() ?? true)
        {
            return Task.FromResult(new ClaimsPrincipal());
        }

        var identity = new ClaimsIdentity(principal!.IdentityProvider);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal?.UserId ?? string.Empty));
        identity.AddClaim(new Claim(ClaimTypes.Name, principal?.UserDetails ?? string.Empty));
        identity.AddClaims(principal?.UserRoles?.Select(r => new Claim(ClaimTypes.Role, r)) ?? new List<Claim>());

        return Task.FromResult(new ClaimsPrincipal(identity));
    }
}