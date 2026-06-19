namespace AnimeFeedManager.Web.Features.Admin;

/// <summary>
/// Empty form model. A no-data trigger still posts as a <c>&lt;form&gt;</c> so the antiforgery
/// token rides along; binding it with <c>[FromForm]</c> is what makes the minimal-API endpoint
/// enforce antiforgery validation (a bodiless endpoint is silently exempt).
/// </summary>
public sealed class Noop;
