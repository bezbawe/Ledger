using System.ComponentModel.DataAnnotations;

namespace Ledger.Domain;

public class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
}
