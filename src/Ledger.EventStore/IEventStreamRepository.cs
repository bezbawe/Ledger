namespace Ledger.EventStore;

public interface IEventStreamRepository
{
    Task<IReadOnlyList<EventRecord>> ReadStreamAsync(Guid streamId, CancellationToken ct = default);

    Task AppendAsync(Guid streamId, long expectedVersion, IReadOnlyList<NewEvent> events, CancellationToken ct = default);
}
