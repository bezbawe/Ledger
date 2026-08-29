using Ledger.Application.Persistence;
using Ledger.Application.Serialization;
using Ledger.Domain;
using Ledger.EventStore;
using Ledger.Projections;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Application.Accounts;

// Координирует write- и read-стороны: событие и обновление проекции сохраняются
// одним вызовом SaveChangesAsync на общем LedgerDbContext — синхронно, в одной транзакции.
public sealed class AccountUnitOfWork
{
    private readonly LedgerDbContext _db;
    private readonly IEventStreamRepository _events;
    private readonly IAccountBalanceRepository _balances;

    public AccountUnitOfWork(LedgerDbContext db, IEventStreamRepository events, IAccountBalanceRepository balances)
    {
        _db = db;
        _events = events;
        _balances = balances;
    }

    public async Task<Account?> LoadAsync(Guid accountId, CancellationToken ct)
    {
        var history = await _events.ReadStreamAsync(accountId, ct);
        if (history.Count == 0)
            return null;

        var domainEvents = history.Select(r => AccountEventSerializer.Deserialize(r.EventType, r.Data));
        return Account.Rehydrate(accountId, domainEvents);
    }

    public async Task SaveAsync(Account account, long expectedVersion, CancellationToken ct)
    {
        var newEvents = account.UncommittedEvents
            .Select(AccountEventSerializer.Serialize)
            .Select(x => new NewEvent(x.EventType, x.Data))
            .ToList();

        await _events.AppendAsync(account.Id, expectedVersion, newEvents, ct);
        await _balances.ApplyAsync(account.Id, account.Balance, account.Version, ct);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            throw new EventStreamConcurrencyException(account.Id, expectedVersion);
        }
    }
}
