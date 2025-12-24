using System.ComponentModel.DataAnnotations;

namespace AnimeFeedManager.Web.Features.Security;

public sealed class LoginViewModel
{
    [Required(AllowEmptyStrings = false)]
    public string Id { get; set; } = string.Empty;
    [Required(ErrorMessage = "Please provide your username", AllowEmptyStrings = false)]
    public string Alias { get; set; } = string.Empty;
}

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Please provide an Email", AllowEmptyStrings = false)]
    [EmailAddress(ErrorMessage = "Please provide a valid Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please provide a display name", AllowEmptyStrings = false)]
    public string DisplayName { get; set; } = string.Empty;

    public string? Token { get; set; }

    public string? UserId { get; set; }
}


public sealed class AddCredentialsViewModel
{

    [Required(ErrorMessage = "Please provide an Id", AllowEmptyStrings = false)]
    public string Id { get; set; } = string.Empty;

    public string? Token { get; set; }
}