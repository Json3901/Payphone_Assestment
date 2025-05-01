using Payphone_Assestment.Domain.Enums;

namespace Payphone_Assestment.Domain.Entities;

public class Transaction : BaseEntity
{
    public int WalletId { get; set; }
    public decimal Amount { get; set; }
    public string OperationType { get; set; }
    public DateTime OperationDate { get; set; }
}