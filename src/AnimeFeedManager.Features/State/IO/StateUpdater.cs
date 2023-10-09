using AnimeFeedManager.Common.Domain.Errors;
using AnimeFeedManager.Common.Domain.Notifications.Base;

namespace AnimeFeedManager.Features.State.IO
{
    public interface IStateUpdater
    {
        public Task<Either<DomainError, CurrentState>> Update<T>(Either<DomainError, T> result, StateChange change,
            CancellationToken token = default);
    }

    public sealed class StateUpdater : IStateUpdater
    {
        private readonly ITableClientFactory<StateUpdateStorage> _tableClientFactory;

        public StateUpdater(
            ITableClientFactory<StateUpdateStorage> tableClientFactory)
        {
            _tableClientFactory = tableClientFactory;
        }


        private Task<Either<DomainError, CurrentState>> UpdateCompleted(string id, NotificationTarget target, string item,
            CancellationToken token = default)
        {
            StateUpdateStorage Add(StateUpdateStorage original)
            {
                original.Completed += 1;
                original.Items = !string.IsNullOrWhiteSpace(original.Items) ? string.Join(',', original.Items, item) : item;
                return original;
            }

            return _tableClientFactory.GetClient()
                .BindAsync(client => TryUpdate(client, id, target, Add, token));
        }

        private Task<Either<DomainError, CurrentState>> UpdateError(string id, NotificationTarget target,
            CancellationToken token = default)
        {
            StateUpdateStorage Add(StateUpdateStorage original)
            {
                original.Errors += 1;
                return original;
            }

            return _tableClientFactory.GetClient()
                .BindAsync(client => TryUpdate(client, id, target, Add, token));
        }


        private static async Task<Either<DomainError, CurrentState>> TryUpdate(
            TableClient client,
            string id,
            NotificationTarget target,
            Func<StateUpdateStorage, StateUpdateStorage> modifier,
            CancellationToken token)
        {
            var keepTrying = true;
            var finalResult =
                Left<DomainError, CurrentState>(
                    new BasicError("Failure Updating State"));
            do
            {
                try
                {
                    finalResult = await GetCurrent(client, id, target)
                        .MapAsync(modifier)
                        .MapAsync(s => Update(client, s, token));

                    keepTrying = false;
                }
                catch (RequestFailedException e)
                {
                    if (e.Status == 412) continue; // not because to pessimistic concurrency
                    keepTrying = false;
                    finalResult = Left<DomainError, CurrentState>(
                        ExceptionError.FromException(e));
                }
                catch (Exception ex)
                {
                    keepTrying = false;
                    finalResult = Left<DomainError, CurrentState>(
                        ExceptionError.FromException(ex));
                }
            } while (keepTrying);

            return finalResult;
        }

        private static Task<Either<DomainError, StateUpdateStorage>> GetCurrent(
            TableClient client,
            string id,
            NotificationTarget target)
        {
            return TableUtils.TryExecute(() => client.GetEntityAsync<StateUpdateStorage>(target.Value, id))
                .MapAsync(r => r.Value);
        }

        private static async Task<CurrentState> Update(
            TableClient client,
            StateUpdateStorage updateStateStorage,
            CancellationToken token)
        {
            await client.UpdateEntityAsync(updateStateStorage, updateStateStorage.ETag, cancellationToken: token);
            return new CurrentState(
                updateStateStorage.RowKey!,
                updateStateStorage.Completed,
                updateStateStorage.Errors,
                updateStateStorage.Items ?? string.Empty,
                updateStateStorage.ToUpdate > 0 && updateStateStorage.ToUpdate ==
                updateStateStorage.Completed + updateStateStorage.Errors);
        }

        public Task<Either<DomainError, CurrentState>> Update<T>(Either<DomainError, T> result, StateChange change,
            CancellationToken token = default)
        {
            return result.MatchAsync(
                _ => UpdateCompleted(change.StateId, change.Target, change.Item, token),
                _ => UpdateError(change.StateId, change.Target, token));
        }
    }
}