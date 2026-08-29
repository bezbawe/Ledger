namespace Ledger.EventStore;

public sealed class EventStreamConcurrencyException : Exception
{
    public EventStreamConcurrencyException(Guid streamId, long expectedVersion)
        : base($"Concurrent write conflict on stream {streamId}: expected version {expectedVersion}.")
    {
    }
}
