namespace AnimeFeedManager.Features.Library.Types;

/// <summary>
/// Broadcast schedule for episodically-aired series. Stored as raw Jikan strings;
/// values like <c>"Saturdays"</c>, <c>"23:30"</c>, <c>"Asia/Tokyo"</c>.
/// </summary>
public readonly record struct Broadcast(string Day, string Time, string Timezone);
