namespace AnimeFeedManager.Features.Scrapping.AnimeSchedule;

[JsonSerializable(typeof(AnimeScheduleResponse))]
[JsonSerializable(typeof(AnimeScheduleAnime))]
[JsonSerializable(typeof(AnimeScheduleSeason))]
[JsonSerializable(typeof(AnimeScheduleNames))]
[JsonSerializable(typeof(AnimeScheduleMediaType))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal partial class AnimeScheduleJsonContext : JsonSerializerContext;
