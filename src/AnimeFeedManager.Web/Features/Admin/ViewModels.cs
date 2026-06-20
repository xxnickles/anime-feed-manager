namespace AnimeFeedManager.Web.Features.Admin;

/// <summary>
/// Empty form model. A no-data trigger still posts as a <c>&lt;form&gt;</c> so the antiforgery
/// token rides along; binding it with <c>[FromForm]</c> is what makes the minimal-API endpoint
/// enforce antiforgery validation (a bodiless endpoint is silently exempt).
/// </summary>
public sealed class Noop;

/// <summary>
/// Custom-season import form. Year is bound leniently (as a string) so a missing / empty /
/// non-numeric value flows through domain validation as a "Year"-tagged field error rather
/// than failing minimal-API binding with a raw 400. Property names match the domain field
/// keys (<c>DomainValidationError.Field == typeof(T).Name</c>): <c>Season</c>, <c>Year</c>.
/// </summary>
public sealed class SeasonImportForm
{
    public string? Season { get; set; }
    public string? Year { get; set; }
}
