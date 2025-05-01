namespace Payphone_Assestment.Domain.Entities;

public class Transaction
{
    public int Id { get; set; }
    public int WalletId { get; set; }
    public decimal Amount { get; set; }
    public int OperationType { get; set; }
    public DateTime OperationDate { get; set; }
}