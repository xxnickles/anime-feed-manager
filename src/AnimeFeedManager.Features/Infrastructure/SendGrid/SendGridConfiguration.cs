namespace AnimeFeedManager.Features.Infrastructure.SendGrid;

public record SendGridConfiguration(string FromEmail, string FromName, bool Sandbox);
