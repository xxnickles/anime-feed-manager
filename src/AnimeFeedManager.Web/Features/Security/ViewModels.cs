using System.ComponentModel.DataAnnotations;

namespace AnimeFeedManager.Web.Features.Security;

public sealed class LoginViewModel
{
    // The Passwordless authentication token from the browser sign-in. The server verifies it and
    // derives the user id from the verified result — the client never supplies an identity.
    [Required(AllowEmptyStrings = false)]
    public string Token { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = string.Empty;
}

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Please provide an email", AllowEmptyStrings = false)]
    [EmailAddress(ErrorMessage = "Please provide a valid email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide a display name", AllowEmptyStrings = false)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Token { get; set; }

    public string? UserId { get; set; }
}

public sealed class AddCredentialsViewModel
{
    [Required(ErrorMessage = "Please provide an id", AllowEmptyStrings = false)]
    public string Id { get; set; } = string.Empty;

    public string? Token { get; set; }
}
