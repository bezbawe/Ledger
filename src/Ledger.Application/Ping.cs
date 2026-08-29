using MediatR;

namespace Ledger.Application;

// Каркас-проба: подтверждает, что MediatR-конвейер собран и команда доходит до хендлера.
// Будет заменена реальными командами (OpenAccount/Deposit/Withdraw) в Этапе 1.
public record Ping(string Message) : IRequest<string>;

public class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        => Task.FromResult($"pong: {request.Message}");
}
