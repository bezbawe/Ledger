using Ledger.EventStore;
using MediatR;

namespace Ledger.Application.Accounts;

public sealed record GetAccountHistoryQuery(Guid AccountId) : IRequest<IReadOnlyList<AccountHistoryEntryDto>>;

public sealed record AccountHistoryEntryDto(long Version, string EventType, string Data, DateTime Timestamp);

public sealed class GetAccountHistoryQueryHandler : IRequestHandler<GetAccountHistoryQuery, IReadOnlyList<AccountHistoryEntryDto>>
{
    private readonly IEventStreamRepository _events;

    public GetAccountHistoryQueryHandler(IEventStreamRepository events)
    {
        _events = events;
    }

    public async Task<IReadOnlyList<AccountHistoryEntryDto>> Handle(GetAccountHistoryQuery request, CancellationToken ct)
    {
        var history = await _events.ReadStreamAsync(request.AccountId, ct);
        return history.Select(e => new AccountHistoryEntryDto(e.Version, e.EventType, e.Data, e.Timestamp)).ToList();
    }
}
