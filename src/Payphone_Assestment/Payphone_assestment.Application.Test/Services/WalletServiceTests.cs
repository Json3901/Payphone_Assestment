using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using Payphone_Assestment.Application.Dtos.Wallet;
using Payphone_Assestment.Application.Services;
using Payphone_Assestment.Domain.Entities;
using Payphone_Assestment.Application.Interfaces.Infrastructure;

namespace Payphone_assestment.Application.Test.Services;

public class WalletServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IGenericRepository<Wallet>> _walletRepoMock;
    private readonly Mock<IGenericRepository<Transaction>> _transactionRepoMock;
    private readonly WalletService _walletService;

    private const int USER_ID = 1;

    public WalletServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _walletRepoMock = new Mock<IGenericRepository<Wallet>>();
        _transactionRepoMock = new Mock<IGenericRepository<Transaction>>();

        _unitOfWorkMock.Setup(u => u.Repository<Wallet>()).Returns(_walletRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Repository<Transaction>()).Returns(_transactionRepoMock.Object);

        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, USER_ID.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);
        var context = new DefaultHttpContext { User = user };
        _httpContextAccessorMock.Setup(h => h.HttpContext).Returns(context);

        _walletService = new WalletService(_unitOfWorkMock.Object, _httpContextAccessorMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnWalletResponse()
    {
        var request = new CreateWalletRequest { Name = "Main", Description = "Primary wallet", InitialBalance = 100 };
        var wallet = new Wallet
            { Id = 1, UserId = USER_ID, Name = "Main", Description = "Primary wallet", Balance = 100 };

        _walletRepoMock.Setup(r => r.AddAsync(It.IsAny<Wallet>())).ReturnsAsync(wallet);

        var result = await _walletService.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Main", result.Name);
    }

    [Fact]
    public async Task GetAllByUserAsync_ShouldReturnWallets()
    {
        var wallets = new List<Wallet>
        {
            new Wallet { Id = 1, Name = "A", Description = "Desc", UserId = USER_ID, Balance = 100 }
        };

        _walletRepoMock.Setup(r => r.GetByFilterAsync(It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(wallets);

        var result = await _walletService.GetAllByUserAsync();

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnWallet()
    {
        var wallet = new Wallet { Id = 1, UserId = USER_ID, Name = "A", Balance = 100 };

        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wallet);

        var result = await _walletService.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal("A", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenWalletIsNull()
    {
        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Wallet)null);

        var result = await _walletService.GetByIdAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenWalletIsDeleted()
    {
        var wallet = new Wallet { Id = 1, UserId = USER_ID, IsDeleted = true };

        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wallet);

        var result = await _walletService.GetByIdAsync(1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenWalletNotOwnedByUser()
    {
        var wallet = new Wallet { Id = 1, UserId = 999 }; // otro usuario

        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wallet);

        var result = await _walletService.GetByIdAsync(1);

        Assert.Null(result);
    }


    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndReturnTrue()
    {
        var wallet = new Wallet { Id = 1, UserId = USER_ID, Name = "Old", Description = "Desc" };
        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wallet);
        _walletRepoMock.Setup(r => r.UpdateAsync(wallet)).ReturnsAsync(true);

        var request = new UpdateWalletRequest { Name = "New", Description = "New Desc", Disable = false };
        var result = await _walletService.UpdateAsync(1, request);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenWalletIsNull()
    {
        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Wallet)null);

        var dto = new UpdateWalletRequest { Name = "New Name" };

        var result = await _walletService.UpdateAsync(1, dto);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenWalletNotOwnedByUser()
    {
        var wallet = new Wallet { Id = 1, UserId = 999 };

        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wallet);

        var dto = new UpdateWalletRequest { Name = "New Name" };

        var result = await _walletService.UpdateAsync(1, dto);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenWalletIsDeleted()
    {
        var wallet = new Wallet { Id = 1, UserId = USER_ID, IsDeleted = true };

        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wallet);

        var dto = new UpdateWalletRequest { Name = "New Name" };

        var result = await _walletService.UpdateAsync(1, dto);

        Assert.False(result);
    }


    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteAndReturnTrue()
    {
        var wallet = new Wallet { Id = 1, UserId = USER_ID };
        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wallet);
        _walletRepoMock.Setup(r => r.SoftDeleteAsync(1)).ReturnsAsync(true);

        var result = await _walletService.DeleteAsync(1);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenWalletIsNull()
    {
        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Wallet)null);

        var result = await _walletService.DeleteAsync(1);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenWalletNotOwnedByUser()
    {
        var wallet = new Wallet { Id = 1, UserId = 999 };

        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(wallet);

        var result = await _walletService.DeleteAsync(1);

        Assert.False(result);
    }

    [Fact]
    public async Task TransferAsync_ShouldTransferFundsAndReturnTrue()
    {
        var fromWallet = new Wallet { Id = 1, UserId = USER_ID, Balance = 200 };
        var toWallet = new Wallet { Id = 2, UserId = 2, Balance = 100 };
        var transaction = new Transaction();

        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fromWallet);
        _walletRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(toWallet);
        _transactionRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>())).ReturnsAsync(transaction);
        _walletRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Wallet>())).ReturnsAsync(true);

        var request = new TransferRequest { FromWalletId = 1, ToWalletId = 2, Amount = 50 };

        var result = await _walletService.TransferAsync(request);

        Assert.True(result);
    }

    [Fact]
    public async Task TransferAsync_ShouldThrowException_WhenAmountIsNegative()
    {
        var request = new TransferRequest { FromWalletId = 1, ToWalletId = 2, Amount = -50 };

        var ex = await Assert.ThrowsAsync<Exception>(() => _walletService.TransferAsync(request));

        Assert.Equal("Transfer amount not valid.", ex.Message);
    }

    [Fact]
    public async Task TransferAsync_ShouldThrowException_WhenWalletsInvalidOrNotOwned()
    {
        var fromWallet = new Wallet { Id = 1, UserId = USER_ID, IsDisabled = true };
        var toWallet = new Wallet { Id = 2, UserId = 2 };

        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fromWallet);
        _walletRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(toWallet);

        var request = new TransferRequest { FromWalletId = 1, ToWalletId = 2, Amount = 50 };

        var ex = await Assert.ThrowsAsync<Exception>(() => _walletService.TransferAsync(request));

        Assert.Equal("Wallet not found or not owned by the user.", ex.Message);
    }

    [Fact]
    public async Task TransferAsync_ShouldThrowException_WhenInsufficientBalance()
    {
        var fromWallet = new Wallet { Id = 1, UserId = USER_ID, Balance = 20 };
        var toWallet = new Wallet { Id = 2, UserId = 2 };

        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fromWallet);
        _walletRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(toWallet);

        var request = new TransferRequest { FromWalletId = 1, ToWalletId = 2, Amount = 50 };

        var ex = await Assert.ThrowsAsync<Exception>(() => _walletService.TransferAsync(request));

        Assert.Equal("Insufficient balance.", ex.Message);
    }

    [Fact]
    public async Task TransferAsync_ShouldThrowGenericException_OnUnhandledError()
    {
        var fromWallet = new Wallet { Id = 1, UserId = USER_ID, Balance = 100 };
        var toWallet = new Wallet { Id = 2, UserId = 2, Balance = 100 };

        _walletRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(fromWallet);
        _walletRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(toWallet);
        _transactionRepoMock.Setup(r => r.AddAsync(It.IsAny<Transaction>()))
            .ThrowsAsync(new Exception("Database error"));

        var request = new TransferRequest { FromWalletId = 1, ToWalletId = 2, Amount = 50 };

        var ex = await Assert.ThrowsAsync<Exception>(() => _walletService.TransferAsync(request));

        Assert.Equal("Database error", ex.Message);
    }
}