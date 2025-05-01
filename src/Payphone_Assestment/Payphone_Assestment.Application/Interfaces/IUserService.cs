using Payphone_Assestment.Application.Dtos.User;

namespace Payphone_Assestment.Application.Interfaces;

public interface IUserService
{
    Task<int> RegisterAsync(RegisterUserRequest user, string password);
    Task<string?> LoginAsync(LoginRequest request);
}