namespace Payphone_Assestment.Domain.Entities;

public class Wallet : BaseEntity
{
    public int UserId { get; set; }
    public string Name { get; set; } = null!;
    public decimal Balance { get; set; }
    public bool IsDisabled { get; set; }
}