namespace Payphone_Assestment.Application.Dtos.Wallet;

public class CreateWalletRequest
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal InitialBalance { get; set; } = 0;
}