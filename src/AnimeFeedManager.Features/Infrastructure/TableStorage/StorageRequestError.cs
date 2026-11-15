namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

/// <summary>A validated request that reads or writes storage.</summary>
[Widens]
public partial union StorageRequestError(ValidationError, StorageError);
