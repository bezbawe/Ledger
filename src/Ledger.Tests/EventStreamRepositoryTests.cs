using Ledger.Application;
using Ledger.Application.Persistence;
using Ledger.EventStore;
using Microsoft.EntityFrameworkCore;

namespace Ledger.Tests;

public class EventStreamRepositoryTests : BaseLedgerTests
{
    private readonly IEventStreamRepository _repository;

    public EventStreamRepositoryTests()
    {
        ServiceCollection.AddLedgerApplication(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
        _repository = GetInstance<IEventStreamRepository>();
    }

    [Fact]
    public async Task Append_Then_Read_Returns_Events_In_Version_Order()
    {
        var streamId = Guid.NewGuid();

        await _repository.AppendAsync(streamId, expectedVersion: 0, new[] { new NewEvent("AccountOpened", "{}") });
        await SaveAsync();
        await _repository.AppendAsync(streamId, expectedVersion: 1, new[] { new NewEvent("MoneyDeposited", "{\"Amount\":100}") });
        await SaveAsync();

        var history = await _repository.ReadStreamAsync(streamId);

        Assert.Equal(2, history.Count);
        Assert.Equal(1, history[0].Version);
        Assert.Equal(2, history[1].Version);
        Assert.Equal("AccountOpened", history[0].EventType);
        Assert.Equal("MoneyDeposited", history[1].EventType);
    }

    [Fact]
    public async Task Two_Commands_From_Same_Version_One_Succeeds_Other_Gets_Conflict()
    {
        var streamId = Guid.NewGuid();
        await _repository.AppendAsync(streamId, expectedVersion: 0, new[] { new NewEvent("AccountOpened", "{}") });
        await SaveAsync();

        // Обе "команды" прочитали стрим на версии 1.
        const long versionSeenByBothCommands = 1;

        await _repository.AppendAsync(streamId, versionSeenByBothCommands, new[] { new NewEvent("MoneyDeposited", "{}") });
        await SaveAsync(); // первая команда проходит

        await Assert.ThrowsAsync<EventStreamConcurrencyException>(() =>
            _repository.AppendAsync(streamId, versionSeenByBothCommands, new[] { new NewEvent("MoneyWithdrawn", "{}") }));
        // вторая команда всё ещё использует устаревшую версию — отклонена, дыр/дублей в Version нет

        var history = await _repository.ReadStreamAsync(streamId);
        Assert.Equal(2, history.Count);
        Assert.Equal(new long[] { 1, 2 }, history.Select(e => e.Version));
    }

    private Task SaveAsync() => GetInstance<LedgerDbContext>().SaveChangesAsync();
}
