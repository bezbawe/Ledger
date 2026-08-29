using Ledger.Domain;

namespace Ledger.EventStore;

public sealed class EventRecord : BaseEntity
{
    public Guid StreamId { get; set; }
    public long Version { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
