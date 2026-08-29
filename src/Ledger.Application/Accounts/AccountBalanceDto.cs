namespace Ledger.Application.Accounts;

public sealed record AccountBalanceDto(Guid AccountId, decimal Balance, long Version);
