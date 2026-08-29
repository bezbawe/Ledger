using Microsoft.Extensions.DependencyInjection;

namespace Ledger.Tests;

// По образцу BaseTatTests из tat.domain: DI-контейнер тестов + резолв через GetInstance<T>().
public class BaseLedgerTests
{
    protected ServiceCollection ServiceCollection { get; }
    protected ServiceProvider Sp { get; private set; } = null!;
    private bool _isBuilt;

    public BaseLedgerTests()
    {
        ServiceCollection = new ServiceCollection();
    }

    public T GetInstance<T>(bool rebuild = false)
    {
        if (rebuild || !_isBuilt)
        {
            Sp = ServiceCollection.BuildServiceProvider();
            _isBuilt = true;
        }

        var instance = Sp.GetService<T>();
        if (instance == null)
            throw new Exception("Error of object initialization using DI");

        return instance;
    }
}
