using Ledger.Application;
using Ledger.Application.Persistence;
using Ledger.Projections;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Tests;

public class AccountBalanceRepositoryTests : BaseLedgerTests
{
    private readonly IAccountBalanceRepository _repository;

    public AccountBalanceRepositoryTests()
    {
        ServiceCollection.AddLedgerApplication(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        _repository = GetInstance<IAccountBalanceRepository>();
    }

    [Fact]
    public async Task Apply_First_Event_Creates_Projection_Row()
    {
        var accountId = Guid.NewGuid();

        await _repository.ApplyAsync(accountId, balance: 0m, version: 1);
        await SaveAsync();

        var balance = await _repository.GetAsync(accountId);
        Assert.NotNull(balance);
        Assert.Equal(0m, balance!.Balance);
        Assert.Equal(1, balance.Version);
    }

    [Fact]
    public async Task Apply_Next_Event_In_Order_Updates_Balance()
    {
        var accountId = Guid.NewGuid();
        await _repository.ApplyAsync(accountId, 0m, 1);
        await SaveAsync();

        await _repository.ApplyAsync(accountId, 100m, 2);
        await SaveAsync();

        var balance = await _repository.GetAsync(accountId);
        Assert.Equal(100m, balance!.Balance);
        Assert.Equal(2, balance.Version);
    }

    [Fact]
    public async Task Apply_Same_Event_Twice_Is_Idempotent()
    {
        var accountId = Guid.NewGuid();
        await _repository.ApplyAsync(accountId, 0m, 1);
        await SaveAsync();
        await _repository.ApplyAsync(accountId, 100m, 2);
        await SaveAsync();

        await _repository.ApplyAsync(accountId, 999m, 2); // повторная доставка того же события
        await SaveAsync();

        var balance = await _repository.GetAsync(accountId);
        Assert.Equal(100m, balance!.Balance);
        Assert.Equal(2, balance.Version);
    }

    private Task SaveAsync() => GetInstance<LedgerDbContext>().SaveChangesAsync();
}
