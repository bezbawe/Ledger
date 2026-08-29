using Ledger.Domain;

namespace Ledger.Projections;

public sealed class AccountBalance : BaseEntity
{
    public Guid AccountId { get; set; }
    public decimal Balance { get; set; }
    public long Version { get; set; }
    public DateTime UpdatedAt { get; set; }
}
