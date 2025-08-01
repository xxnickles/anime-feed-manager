using System.ComponentModel.DataAnnotations;

namespace AnimeFeedManager.Web.Features.Security;

public sealed class LoginViewModel
{
    [Required(AllowEmptyStrings = false)]
    public string Id { get; set; } = string.Empty;
    [Required(ErrorMessage = "Please provide your username", AllowEmptyStrings = false)]
    public string Alias { get; set; } = string.Empty;
    
}