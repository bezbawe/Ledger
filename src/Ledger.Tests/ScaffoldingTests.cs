using Ledger.Application;
using Ledger.EventStore;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ledger.Tests;

public class ScaffoldingTests : BaseLedgerTests
{
    [Fact]
    public void Harness_Is_Green()
    {
        Assert.True(true);
    }

    [Fact]
    public void GetInstance_Resolves_Registered_DbContext_On_InMemory_Provider()
    {
        ServiceCollection.AddDbContext<LedgerDbContext>(o => o.UseInMemoryDatabase("scaffold-test"));

        var db = GetInstance<LedgerDbContext>();

        Assert.NotNull(db);
        Assert.True(db.Database.CanConnect());
    }

    [Fact]
    public async Task Mediatr_Command_Reaches_Handler()
    {
        ServiceCollection.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Ping).Assembly));

        var mediator = GetInstance<IMediator>();
        var result = await mediator.Send(new Ping("scaffold"));

        Assert.Equal("pong: scaffold", result);
    }
}
