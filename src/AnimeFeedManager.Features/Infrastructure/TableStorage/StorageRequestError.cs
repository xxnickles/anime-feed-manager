namespace AnimeFeedManager.Features.Infrastructure.TableStorage;

/// <summary>A validated request that reads or writes storage.</summary>
public union StorageRequestError(ValidationError, StorageError);
