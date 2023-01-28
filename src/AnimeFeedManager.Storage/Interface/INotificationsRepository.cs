using System.Collections.Immutable;
using AnimeFeedManager.Common.Notifications;
using AnimeFeedManager.Storage.Domain;

namespace AnimeFeedManager.Storage.Interface;

public interface INotificationsRepository
{
    Task<Either<DomainError, ImmutableList<NotificationStorage>>> Get(string userId);
    
    Task<Either<DomainError, ImmutableList<NotificationStorage>>> GetForAdmin(string userId);
    Task<Either<DomainError, Unit>> Merge<T>(string id, string userId, NotificationFor @for, NotificationType type, T payload);
}