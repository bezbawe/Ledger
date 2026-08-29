using Ledger.Projections;
using MediatR;

namespace Ledger.Application.Accounts;

public sealed record GetAccountBalanceQuery(Guid AccountId) : IRequest<AccountBalanceDto?>;

public sealed class GetAccountBalanceQueryHandler : IRequestHandler<GetAccountBalanceQuery, AccountBalanceDto?>
{
    private readonly IAccountBalanceRepository _balances;

    public GetAccountBalanceQueryHandler(IAccountBalanceRepository balances)
    {
        _balances = balances;
    }

    public async Task<AccountBalanceDto?> Handle(GetAccountBalanceQuery request, CancellationToken ct)
    {
        var balance = await _balances.GetAsync(request.AccountId, ct);
        return balance is null ? null : new AccountBalanceDto(balance.AccountId, balance.Balance, balance.Version);
    }
}
