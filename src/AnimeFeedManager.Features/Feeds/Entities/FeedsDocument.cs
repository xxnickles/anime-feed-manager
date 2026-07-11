using static AnimeFeedManager.Features.Feeds.Entities.Constants;

namespace AnimeFeedManager.Features.Feeds.Entities;

/// <summary>
/// Abstract base for every document in the <c>feeds</c> container. Polymorphic via STJ
/// <see cref="JsonPolymorphicAttribute"/>; one container, partitioned so a series' classification,
/// confirmation markers, and release history all share a partition, and a source's checkpoint
/// and run history share a partition. Each per-series document type is written by exactly one
/// job, avoiding cross-job write contention on a shared document.
/// </summary>
[CosmosEntity(CosmosContainers.Feeds, FeedsPartitionKey)]
[JsonPolymorphic(TypeDiscriminatorPropertyName = "docType")]
[JsonDerivedType(typeof(SeriesClassification), "classification")]
[JsonDerivedType(typeof(NyaaConfirmation), "nyaa-confirmation")]
[JsonDerivedType(typeof(AiringClockFlag), "airing-clock-flag")]
[JsonDerivedType(typeof(CollectionCheckpoint), "checkpoint")]
[JsonDerivedType(typeof(CollectionRun), "run")]
[JsonDerivedType(typeof(ReleaseDetected), "release-detected")]
public abstract record FeedsDocument : CosmosDocument
{
    public required string PartitionKey { get; init; }
}
