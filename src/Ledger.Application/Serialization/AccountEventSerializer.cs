using System.Text.Json;
using Ledger.Domain;

namespace Ledger.Application.Serialization;

internal static class AccountEventSerializer
{
    public static (string EventType, string Data) Serialize(AccountEvent @event) => @event switch
    {
        AccountOpened e => (nameof(AccountOpened), JsonSerializer.Serialize(e)),
        MoneyDeposited e => (nameof(MoneyDeposited), JsonSerializer.Serialize(e)),
        MoneyWithdrawn e => (nameof(MoneyWithdrawn), JsonSerializer.Serialize(e)),
        _ => throw new InvalidOperationException($"Unknown event type: {@event.GetType().Name}")
    };

    public static AccountEvent Deserialize(string eventType, string data) => eventType switch
    {
        nameof(AccountOpened) => JsonSerializer.Deserialize<AccountOpened>(data)!,
        nameof(MoneyDeposited) => JsonSerializer.Deserialize<MoneyDeposited>(data)!,
        nameof(MoneyWithdrawn) => JsonSerializer.Deserialize<MoneyWithdrawn>(data)!,
        _ => throw new InvalidOperationException($"Unknown event type: {eventType}")
    };
}
