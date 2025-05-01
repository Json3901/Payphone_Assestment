namespace Payphone_Assestment.Application.Dtos.Wallet;

public class UpdateWalletRequest : CreateWalletRequest
{
    public bool Disable { get; set; }
}