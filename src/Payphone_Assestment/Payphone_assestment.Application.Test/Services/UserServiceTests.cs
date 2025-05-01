using Moq;
using Microsoft.Extensions.Configuration;
using Payphone_Assestment.Application.Services;
using Payphone_Assestment.Application.Dtos.User;
using Payphone_Assestment.Domain.Entities;
using Payphone_Assestment.Application.Interfaces.Infrastructure;

namespace Payphone_assestment.Application.Test.Services;

public class UserServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IConfiguration> _configurationMock = new();
    private readonly Mock<IGenericRepository<User>> _userRepoMock = new();

    private readonly UserService _userService;

    public UserServiceTests()
    {
        _unitOfWorkMock.Setup(u => u.Repository<User>()).Returns(_userRepoMock.Object);

        _configurationMock.Setup(conf => conf["Jwt:Key"]).Returns("super_secret_key_123456789_testing_token");
        _configurationMock.Setup(conf => conf["Jwt:Issuer"]).Returns("TestIssuer");
        _configurationMock.Setup(conf => conf["Jwt:Audience"]).Returns("TestAudience");

        _userService = new UserService(_unitOfWorkMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_Should_Create_User()
    {
        var request = new RegisterUserRequest
        {
            Username = "testuser",
            FullName = "Test User"
        };

        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(new User { Id = 1 });

        var result = await _userService.RegisterAsync(request, "password123");

        Assert.True(result);
        _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
        _unitOfWorkMock.Verify(u => u.CommitAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_Should_Throw_And_Rollback_On_Error()
    {
        var request = new RegisterUserRequest { Username = "fail", FullName = "Fail" };

        _userRepoMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Insert failed"));

        await Assert.ThrowsAsync<Exception>(() => _userService.RegisterAsync(request, "password123"));

        _unitOfWorkMock.Verify(u => u.BeginTransaction(), Times.Once);
        _unitOfWorkMock.Verify(u => u.Rollback(), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_Should_Return_Token_When_Credentials_Valid()
    {
        var password = "password123";
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        _userRepoMock.Setup(r => r.GetByFilterAsync(It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<User>
            {
                new User { Id = 1, Username = "user", PasswordHash = hash, UserTypeId = 1 }
            });

        var token = await _userService.LoginAsync(new LoginRequest { Username = "user", Password = password });

        Assert.NotNull(token);
        Assert.Contains(".", token);
    }

    [Fact]
    public async Task LoginAsync_Should_Return_Null_When_User_Not_Found()
    {
        _userRepoMock.Setup(r => r.GetByFilterAsync(It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<User>());

        var result = await _userService.LoginAsync(new LoginRequest { Username = "nouser", Password = "123" });

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_Should_Return_Null_When_Password_Invalid()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("correct_password");

        _userRepoMock.Setup(r => r.GetByFilterAsync(It.IsAny<Dictionary<string, object>>()))
            .ReturnsAsync(new List<User>
            {
                new User { Id = 1, Username = "user", PasswordHash = hash, UserTypeId = 1 }
            });

        var result = await _userService.LoginAsync(new LoginRequest { Username = "user", Password = "wrong" });

        Assert.Null(result);
    }
}