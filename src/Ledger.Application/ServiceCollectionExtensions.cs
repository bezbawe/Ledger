using Ledger.Application.Accounts;
using Ledger.Application.Persistence;
using Ledger.EventStore;
using Ledger.Projections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ledger.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLedgerApplication(this IServiceCollection services, string connectionString) =>
        services.AddLedgerApplication(o => o.UseNpgsql(connectionString));

    // Провайдер-независимая перегрузка: используется тестами для подключения EF InMemory,
    // не открывая AccountUnitOfWork (internal) за пределы сборки.
    public static IServiceCollection AddLedgerApplication(this IServiceCollection services, Action<DbContextOptionsBuilder> configureDb)
    {
        services.AddDbContext<LedgerDbContext>(configureDb);
        services.AddScoped<IEventStreamRepository, EventStreamRepository>();
        services.AddScoped<IAccountBalanceRepository, AccountBalanceRepository>();
        services.AddScoped<AccountUnitOfWork>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        return services;
    }
}
