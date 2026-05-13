namespace AnimeFeedManager.Infrastructure.Cosmos.Results;


public record CosmosResult<T>(T Value, double RequestCharge);

