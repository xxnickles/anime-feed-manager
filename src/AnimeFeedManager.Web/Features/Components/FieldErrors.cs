using AnimeFeedManager.Shared.Results.Errors;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AnimeFeedManager.Web.Features.Components;

/// <summary>
/// Builds the out-of-band response that lands domain validation failures back in their form
/// field slots. The channel split mirrors <see cref="Notifications"/>: field-shaped problems
/// (<see cref="DomainValidationErrors"/>) flow to the inline slots via this helper, while
/// operation / unexpected failures flow to a toast via <see cref="Notifications.Error"/>.
/// The contract: <c>DomainValidationError.Field == typeof(T).Name == input name == slot id</c>.
/// </summary>
internal static class FieldErrors
{
    internal static RazorComponentResult Oob(DomainValidationErrors errors)
        => new RazorComponentResult<FieldErrorsOob>(new Dictionary<string, object?>
        {
            [nameof(FieldErrorsOob.Errors)] = errors.Errors
        });
}
