using Microsoft.EntityFrameworkCore;

namespace Ledger.EventStore;

// Каркас: пустой контекст для проверки связности с БД.
// Таблица Events и её конфигурация добавятся в Этапе 1.
public class LedgerDbContext : DbContext
{
    public LedgerDbContext(DbContextOptions<LedgerDbContext> options) : base(options)
    {
    }
}
