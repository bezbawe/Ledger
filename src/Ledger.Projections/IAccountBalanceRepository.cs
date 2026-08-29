namespace Ledger.Projections;

public interface IAccountBalanceRepository
{
    Task<AccountBalance?> GetAsync(Guid accountId, CancellationToken ct = default);

    // Идемпотентно: применяет только следующее по порядку событие (version == текущая + 1),
    // повторная или устаревшая доставка не меняет проекцию.
    Task ApplyAsync(Guid accountId, decimal balance, long version, CancellationToken ct = default);
}
