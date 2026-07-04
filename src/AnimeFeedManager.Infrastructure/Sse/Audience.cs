namespace AnimeFeedManager.Infrastructure.Sse;

/// <summary>
/// Ordered notification audience. A connection at a given level receives bindings
/// at that level and every level below it (Admin ⊇ Registered ⊇ Public).
/// </summary>
public enum Audience
{
    Public,
    Registered,
    Admin
}
