namespace Payphone_Assestment.Application.Dtos.Wallet;

public class TransferRequest
{
    public int FromWalletId { get; set; }
    public int ToWalletId { get; set; }
    public decimal Amount { get; set; }
}