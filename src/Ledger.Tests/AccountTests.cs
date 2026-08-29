using Ledger.Domain;

namespace Ledger.Tests;

public class AccountTests
{
    [Fact]
    public void Open_Raises_AccountOpened_With_Zero_Balance()
    {
        var accountId = Guid.NewGuid();

        var account = Account.Open(accountId);

        Assert.Equal(accountId, account.Id);
        Assert.Equal(0, account.Balance);
        Assert.True(account.IsOpen);
        Assert.Equal(1, account.Version);
        Assert.Single(account.UncommittedEvents);
        Assert.IsType<AccountOpened>(account.UncommittedEvents[0]);
    }

    [Fact]
    public void Deposit_Increases_Balance_And_Raises_Event()
    {
        var account = Account.Open(Guid.NewGuid());

        account.Deposit(100m);

        Assert.Equal(100m, account.Balance);
        Assert.Equal(2, account.Version);
        Assert.IsType<MoneyDeposited>(account.UncommittedEvents[^1]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Deposit_Throws_When_Amount_Not_Positive(decimal amount)
    {
        var account = Account.Open(Guid.NewGuid());

        Assert.Throws<DomainException>(() => account.Deposit(amount));
    }

    [Fact]
    public void Withdraw_Decreases_Balance_When_Sufficient_Funds()
    {
        var account = Account.Open(Guid.NewGuid());
        account.Deposit(100m);

        account.Withdraw(40m);

        Assert.Equal(60m, account.Balance);
        Assert.Equal(3, account.Version);
        Assert.IsType<MoneyWithdrawn>(account.UncommittedEvents[^1]);
    }

    [Fact]
    public void Withdraw_More_Than_Balance_Throws_And_Does_Not_Raise_Event()
    {
        var account = Account.Open(Guid.NewGuid());
        account.Deposit(50m);

        Assert.Throws<DomainException>(() => account.Withdraw(51m));
        Assert.Equal(50m, account.Balance);
        Assert.Equal(2, account.Version);
        Assert.DoesNotContain(account.UncommittedEvents, e => e is MoneyWithdrawn);
    }

    [Fact]
    public void Rehydrate_Replays_History_Into_Same_State_As_Live_Aggregate()
    {
        var accountId = Guid.NewGuid();
        var live = Account.Open(accountId);
        live.Deposit(100m);
        live.Withdraw(30m);

        var rehydrated = Account.Rehydrate(accountId, live.UncommittedEvents);

        Assert.Equal(live.Balance, rehydrated.Balance);
        Assert.Equal(live.Version, rehydrated.Version);
        Assert.Empty(rehydrated.UncommittedEvents);
    }
}
