using Ledger.Domain;
using MediatR;

namespace Ledger.Application.Accounts;

public sealed record WithdrawCommand(Guid AccountId, decimal Amount) : IRequest<AccountBalanceDto>;

public sealed class WithdrawCommandHandler : IRequestHandler<WithdrawCommand, AccountBalanceDto>
{
    private readonly AccountUnitOfWork _uow;

    public WithdrawCommandHandler(AccountUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<AccountBalanceDto> Handle(WithdrawCommand request, CancellationToken ct)
    {
        var account = await _uow.LoadAsync(request.AccountId, ct)
            ?? throw new DomainException("Account does not exist");

        var expectedVersion = account.Version;
        account.Withdraw(request.Amount);
        await _uow.SaveAsync(account, expectedVersion, ct);

        return new AccountBalanceDto(account.Id, account.Balance, account.Version);
    }
}
