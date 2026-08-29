using Ledger.Domain;
using MediatR;

namespace Ledger.Application.Accounts;

public sealed record OpenAccountCommand(Guid AccountId) : IRequest<AccountBalanceDto>;

public sealed class OpenAccountCommandHandler : IRequestHandler<OpenAccountCommand, AccountBalanceDto>
{
    private readonly AccountUnitOfWork _uow;

    public OpenAccountCommandHandler(AccountUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<AccountBalanceDto> Handle(OpenAccountCommand request, CancellationToken ct)
    {
        var existing = await _uow.LoadAsync(request.AccountId, ct);
        if (existing is not null)
            throw new DomainException("Account already exists");

        var account = Account.Open(request.AccountId);
        await _uow.SaveAsync(account, expectedVersion: 0, ct);

        return new AccountBalanceDto(account.Id, account.Balance, account.Version);
    }
}
