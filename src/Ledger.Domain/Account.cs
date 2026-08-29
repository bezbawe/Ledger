namespace Ledger.Domain;

public sealed class Account
{
    private readonly List<AccountEvent> _uncommittedEvents = new();

    public Guid Id { get; private set; }
    public decimal Balance { get; private set; }
    public bool IsOpen { get; private set; }
    public long Version { get; private set; }

    public IReadOnlyList<AccountEvent> UncommittedEvents => _uncommittedEvents;

    private Account()
    {
    }

    public static Account Open(Guid accountId)
    {
        var account = new Account { Id = accountId };
        account.Raise(new AccountOpened());
        return account;
    }

    public static Account Rehydrate(Guid accountId, IEnumerable<AccountEvent> history)
    {
        var account = new Account { Id = accountId };
        foreach (var @event in history)
        {
            account.Apply(@event);
            account.Version++;
        }

        return account;
    }

    public void Deposit(decimal amount)
    {
        EnsureOpen();
        EnsurePositiveAmount(amount);
        Raise(new MoneyDeposited(amount));
    }

    public void Withdraw(decimal amount)
    {
        EnsureOpen();
        EnsurePositiveAmount(amount);
        if (Balance - amount < 0)
            throw new DomainException("Insufficient funds");

        Raise(new MoneyWithdrawn(amount));
    }

    private void EnsureOpen()
    {
        if (!IsOpen)
            throw new DomainException("Account is not open");
    }

    private static void EnsurePositiveAmount(decimal amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be positive");
    }

    private void Raise(AccountEvent @event)
    {
        Apply(@event);
        Version++;
        _uncommittedEvents.Add(@event);
    }

    private void Apply(AccountEvent @event)
    {
        switch (@event)
        {
            case AccountOpened:
                IsOpen = true;
                Balance = 0;
                break;
            case MoneyDeposited deposited:
                Balance += deposited.Amount;
                break;
            case MoneyWithdrawn withdrawn:
                Balance -= withdrawn.Amount;
                break;
        }
    }
}
