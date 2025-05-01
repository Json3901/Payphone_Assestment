using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Payphone_Assestment.Application.Dtos.Wallet;
using Payphone_Assestment.Application.Interfaces;
using Payphone_Assestment.Application.Interfaces.Infrastructure;
using Payphone_Assestment.Domain.Entities;
using Payphone_Assestment.Domain.Enums;

namespace Payphone_Assestment.Application.Services;

public class WalletService(
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor) : IWalletService
{
    private readonly IGenericRepository<Wallet> _walletRepository = unitOfWork.Repository<Wallet>();
    private readonly IGenericRepository<Transaction> _transactionRepository = unitOfWork.Repository<Transaction>();

    private int GetCurrentUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null ? int.Parse(claim.Value) : throw new UnauthorizedAccessException();
    }

    public async Task<WalletResponse> CreateAsync(CreateWalletRequest request)
    {
        var wallet = new Wallet
        {
            UserId = GetCurrentUserId(),
            Name = request.Name,
            Description = request.Description,
            Balance = request.InitialBalance
        };

        var createdWallet = await _walletRepository.AddAsync(wallet);

        return new WalletResponse
        {
            Id = createdWallet.Id,
            Name = createdWallet.Name,
            Description = createdWallet.Description,
            Balance = createdWallet.Balance,
            IsDisabled = createdWallet.IsDisabled,
            CreatedAt = createdWallet.CreatedAt,
            UpdatedAt = createdWallet.UpdatedAt
        };
    }

    public async Task<IEnumerable<WalletResponse>> GetAllByUserAsync()
    {
        var filters = new Dictionary<string, object>
        {
            { "UserId", GetCurrentUserId() },
            { "IsDeleted", false }
        };

        var wallets = await _walletRepository.GetByFilterAsync(filters);

        return wallets.Select(wallet => new WalletResponse
        {
            Id = wallet.Id,

            Name = wallet.Name,
            Description = wallet.Description,
            Balance = wallet.Balance,
            IsDisabled = wallet.IsDisabled,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        });
    }

    public async Task<WalletResponse?> GetByIdAsync(int id)
    {
        var wallet = await _walletRepository.GetByIdAsync(id);

        if (wallet == null || wallet.IsDeleted || wallet.UserId != GetCurrentUserId())
            return null;

        return new WalletResponse
        {
            Id = wallet.Id,
            Name = wallet.Name,
            Description = wallet.Description,
            Balance = wallet.Balance,
            IsDisabled = wallet.IsDisabled,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        };
    }

    public async Task<bool> UpdateAsync(int id, UpdateWalletRequest request)
    {
        var wallet = await _walletRepository.GetByIdAsync(id);

        if (wallet == null || wallet.UserId != GetCurrentUserId() || wallet.IsDeleted)
            return false;

        wallet.Name = request.Name;
        wallet.Description = request.Description;
        wallet.IsDisabled = request.Disable;

        return await _walletRepository.UpdateAsync(wallet);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var wallet = await _walletRepository.GetByIdAsync(id);

        if (wallet == null || wallet.UserId != GetCurrentUserId())
            return false;

        return await _walletRepository.SoftDeleteAsync(id);
    }

    public async Task<bool> TransferAsync(TransferRequest request)
    {
        if (request.Amount < 0)
            throw new Exception("Transfer amount not valid.");

        try
        {
            var fromWallet = await _walletRepository.GetByIdAsync(request.FromWalletId);
            var toWallet = await _walletRepository.GetByIdAsync(request.ToWalletId);

            if (fromWallet == null || fromWallet.IsDeleted || fromWallet.IsDisabled ||
                fromWallet.UserId != GetCurrentUserId() ||
                toWallet == null || toWallet.IsDeleted || toWallet.IsDisabled)
                throw new Exception("Wallet not found or not owned by the user.");

            if (fromWallet.Balance < request.Amount)
                throw new Exception("Insufficient balance.");

            fromWallet.Balance -= request.Amount;
            toWallet.Balance += request.Amount;

            var fromTransaction = new Transaction
            {
                WalletId = fromWallet.Id,
                Amount = request.Amount,
                OperationType = nameof(OperationType.Credit),
                OperationDate = DateTime.UtcNow
            };

            var toTransaction = new Transaction
            {
                WalletId = toWallet.Id,
                Amount = request.Amount,
                OperationType = nameof(OperationType.Debit),
                OperationDate = DateTime.UtcNow
            };

            var createdTransaction = await _transactionRepository.AddAsync(fromTransaction);
            if (createdTransaction == null) throw new Exception("Failed to create transaction in source.");

            createdTransaction = await _transactionRepository.AddAsync(toTransaction);
            if (createdTransaction == null) throw new Exception("Failed to create transaction in destination wallet.");

            await _walletRepository.UpdateAsync(fromWallet);
            await _walletRepository.UpdateAsync(toWallet);

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Error during transfer. It couldn't finished.");
        }
    }
}