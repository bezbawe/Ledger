namespace Ledger.EventStore;

public sealed record NewEvent(string EventType, string Data);
