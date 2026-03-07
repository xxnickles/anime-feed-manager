#!/usr/bin/env -S dotnet run
#:package Azure.Data.Tables@12.11.0

// Backfills the Season field on existing NotificationSent events in Azure Table Storage.
//
// Usage:
//   dotnet run utils/BackfillNotificationSeason.cs -- "CONNECTION_STRING"
//   AZURE_STORAGE_CONNECTION_STRING="..." dotnet run utils/BackfillNotificationSeason.cs
//
// Or directly after: chmod +x utils/BackfillNotificationSeason.cs
//   ./utils/BackfillNotificationSeason.cs "CONNECTION_STRING"
//   AZURE_STORAGE_CONNECTION_STRING="..." ./utils/BackfillNotificationSeason.cs

using Azure;
using Azure.Data.Tables;
using System.Text.Json;
using System.Text.Json.Serialization;

// ── Constants ──────────────────────────────────────────────────────────────────

const string EventsTable        = "Events";
const string AnimeLibraryTable  = "AnimeLibrary";
const string PayloadTypeName    = "NotificationSent";

// ── Connection string ──────────────────────────────────────────────────────────

var connectionString = args.Length > 0
    ? args[0]
    : Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING")
      ?? throw new InvalidOperationException(
             "Provide connection string as first arg or set AZURE_STORAGE_CONNECTION_STRING.");

var serviceClient = new TableServiceClient(connectionString);

// ── Step 1: Load NotificationSent events that need backfilling ────────────────

Console.WriteLine("Loading NotificationSent events...");

var eventsClient = serviceClient.GetTableClient(EventsTable);
int autoAssigned = 0, userAssigned = 0, alreadyHadSeason = 0, skipped = 0, errors = 0;

// Collect events that need backfilling; track those already done for the summary
var pending = new List<TableEntity>();

await foreach (var entity in eventsClient.QueryAsync<TableEntity>(
    filter: $"PayloadTypeName eq '{PayloadTypeName}'"))
{
    var payloadData = entity.GetString("PayloadData");
    if (string.IsNullOrEmpty(payloadData)) { skipped++; continue; }

    NotificationSent notification;
    try
    {
        notification = JsonSerializer.Deserialize(payloadData, BackfillJsonContext.Default.NotificationSent)
                       ?? throw new InvalidOperationException("Null payload after deserialize.");
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"  [ERROR] RowKey={entity.RowKey}: {ex.Message}");
        errors++;
        continue;
    }

    if (!string.IsNullOrEmpty(notification.Season))
    {
        alreadyHadSeason++;
        continue;
    }

    pending.Add(entity);
}

Console.WriteLine($"Found {pending.Count} events needing backfill ({alreadyHadSeason} already have a season).");

if (pending.Count == 0)
{
    Console.WriteLine("Nothing to do.");
    return;
}

// ── Step 2: Build FeedTitle → seasons lookup for only the titles we need ──────

var neededTitles = pending
    .Select(e => JsonSerializer.Deserialize(e.GetString("PayloadData")!, BackfillJsonContext.Default.NotificationSent)!.Title)
    .ToHashSet(StringComparer.OrdinalIgnoreCase);

Console.WriteLine($"Looking up {neededTitles.Count} distinct title(s) in AnimeLibrary...");

var libraryClient = serviceClient.GetTableClient(AnimeLibraryTable);
var lookup        = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

await foreach (var entity in libraryClient.QueryAsync<TableEntity>())
{
    var feedTitle = entity.GetString("FeedTitle");
    var season    = entity.PartitionKey;

    if (string.IsNullOrEmpty(feedTitle) || string.IsNullOrEmpty(season))
        continue;

    if (!neededTitles.Contains(feedTitle))
        continue;

    if (!lookup.TryGetValue(feedTitle, out var seasons))
        lookup[feedTitle] = seasons = [];

    if (!seasons.Contains(season))
        seasons.Add(season);
}

Console.WriteLine($"Lookup built: {lookup.Count} of {neededTitles.Count} titles matched.");

// ── Step 3: Process pending events ────────────────────────────────────────────

var batchChoices = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

foreach (var entity in pending)
{
    var notification = JsonSerializer.Deserialize(
        entity.GetString("PayloadData")!, BackfillJsonContext.Default.NotificationSent)!;

    // ── Resolve season ────────────────────────────────────────────────────────

    string resolvedSeason;

    if (batchChoices.TryGetValue(notification.Title, out var cached))
    {
        resolvedSeason = cached;
        autoAssigned++;
    }
    else if (!lookup.TryGetValue(notification.Title, out var candidates))
    {
        Console.WriteLine($"  [SKIP] No season found for title '{notification.Title}'");
        skipped++;
        continue;
    }
    else if (candidates.Count == 1)
    {
        resolvedSeason = candidates[0];
        autoAssigned++;
    }
    else
    {
        // Interactive prompt for ambiguous titles
        var episodeLabel = notification.Episodes.Length == 1 ? "episode" : "episodes";
        Console.WriteLine($"\nTitle '{notification.Title}' — {notification.Episodes.Length} {episodeLabel}: {string.Join(", ", notification.Episodes)}");
        Console.WriteLine("Found in multiple seasons:");
        for (int i = 0; i < candidates.Count; i++)
            Console.WriteLine($"  [{i + 1}] {candidates[i]}");
        Console.WriteLine("  (append 'A' to apply to all future occurrences, e.g. '1A')");

        int choiceIndex = -1;
        bool applyBatch = false;

        while (choiceIndex < 0)
        {
            Console.Write("Select season (number): ");
            var input = Console.ReadLine()?.Trim() ?? "";

            applyBatch = input.EndsWith("A", StringComparison.OrdinalIgnoreCase);
            var numberPart = applyBatch ? input[..^1] : input;

            if (int.TryParse(numberPart, out var idx) && idx >= 1 && idx <= candidates.Count)
                choiceIndex = idx - 1;
            else
                Console.WriteLine($"  Invalid input — enter a number between 1 and {candidates.Count}, optionally followed by 'A'.");
        }

        resolvedSeason = candidates[choiceIndex];
        if (applyBatch) batchChoices[notification.Title] = resolvedSeason;
        userAssigned++;
    }

    // ── Re-serialize and update ───────────────────────────────────────────────

    entity["PayloadData"] = JsonSerializer.Serialize(
        notification with { Season = resolvedSeason }, BackfillJsonContext.Default.NotificationSent);

    await eventsClient.UpdateEntityAsync(entity, ETag.All, TableUpdateMode.Replace);
}

// ── Summary ────────────────────────────────────────────────────────────────────

Console.WriteLine($"""

Backfill complete.
  Auto-assigned:      {autoAssigned}
  User-assigned:      {userAssigned}
  Already had season: {alreadyHadSeason}
  Skipped (no match): {skipped}
  Errors:             {errors}
  Total processed:    {autoAssigned + userAssigned + alreadyHadSeason + skipped + errors}
""");

// ── Local types ────────────────────────────────────────────────────────────────

record NotificationSent(string Title, string Url, string? Season, string[] Episodes);

[JsonSerializable(typeof(NotificationSent))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
partial class BackfillJsonContext : JsonSerializerContext;
