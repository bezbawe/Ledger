using Ledger.Projections;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Application.Persistence;

public sealed class AccountBalanceRepository : IAccountBalanceRepository
{
    private readonly LedgerDbContext _db;

    public AccountBalanceRepository(LedgerDbContext db)
    {
        _db = db;
    }

    public Task<AccountBalance?> GetAsync(Guid accountId, CancellationToken ct = default)
    {
        return _db.AccountBalances.SingleOrDefaultAsync(b => b.AccountId == accountId, ct);
    }

    public async Task ApplyAsync(Guid accountId, decimal balance, long version, CancellationToken ct = default)
    {
        var existing = await GetAsync(accountId, ct);

        if (existing is null)
        {
            if (version != 1)
                return;

            _db.AccountBalances.Add(new AccountBalance
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                Balance = balance,
                Version = version,
                UpdatedAt = DateTime.UtcNow
            });
            return;
        }

        if (version != existing.Version + 1)
            return;

        existing.Balance = balance;
        existing.Version = version;
        existing.UpdatedAt = DateTime.UtcNow;
    }
}
