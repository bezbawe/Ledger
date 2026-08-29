using Ledger.EventStore;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Application.Persistence;

// AppendAsync только готовит запись (Add) и проверяет версию — SaveChangesAsync вызывает
// вызывающий код одним разом вместе с обновлением проекции (см. AccountUnitOfWork), чтобы
// событие и read-модель попадали в одну транзакцию, как того требует MVP.
public sealed class EventStreamRepository : IEventStreamRepository
{
    private readonly LedgerDbContext _db;

    public EventStreamRepository(LedgerDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<EventRecord>> ReadStreamAsync(Guid streamId, CancellationToken ct = default)
    {
        return await _db.Events
            .Where(e => e.StreamId == streamId)
            .OrderBy(e => e.Version)
            .ToListAsync(ct);
    }

    public async Task AppendAsync(Guid streamId, long expectedVersion, IReadOnlyList<NewEvent> events, CancellationToken ct = default)
    {
        if (events.Count == 0)
            return;

        var actualVersion = await _db.Events
            .Where(e => e.StreamId == streamId)
            .Select(e => (long?)e.Version)
            .MaxAsync(ct) ?? 0;

        if (actualVersion != expectedVersion)
            throw new EventStreamConcurrencyException(streamId, expectedVersion);

        var version = expectedVersion;
        var timestamp = DateTime.UtcNow;
        foreach (var newEvent in events)
        {
            version++;
            _db.Events.Add(new EventRecord
            {
                Id = Guid.NewGuid(),
                StreamId = streamId,
                Version = version,
                EventType = newEvent.EventType,
                Data = newEvent.Data,
                Timestamp = timestamp
            });
        }
    }
}
