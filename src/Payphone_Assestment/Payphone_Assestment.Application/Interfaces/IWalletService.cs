using Payphone_Assestment.Application.Dtos.Wallet;

namespace Payphone_Assestment.Application.Interfaces;

public interface IWalletService
{
    Task<WalletResponse> CreateAsync(CreateWalletRequest request);
    Task<IEnumerable<WalletResponse>> GetAllByUserAsync();
    Task<WalletResponse?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(int id, UpdateWalletRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> TransferAsync(TransferRequest request);
}