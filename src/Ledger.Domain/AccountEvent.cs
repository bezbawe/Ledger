namespace Ledger.Domain;

public abstract record AccountEvent;

public sealed record AccountOpened : AccountEvent;

public sealed record MoneyDeposited(decimal Amount) : AccountEvent;

public sealed record MoneyWithdrawn(decimal Amount) : AccountEvent;
