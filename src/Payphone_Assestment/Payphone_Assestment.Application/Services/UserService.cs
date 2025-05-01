using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Payphone_Assestment.Application.Dtos.User;
using Payphone_Assestment.Application.Interfaces;
using Payphone_Assestment.Application.Interfaces.Infrastructure;
using Payphone_Assestment.Domain.Entities;
using Payphone_Assestment.Domain.Enums;

namespace Payphone_Assestment.Application.Services;

public class UserService(IUnitOfWork unitOfWork, IConfiguration configuration) : IUserService
{
    public async Task<bool> RegisterAsync(RegisterUserRequest request, string password)
    {
        unitOfWork.BeginTransaction();
        var user = new User
        {
            UserTypeId = 2,
            Username = request.Username,
            FullName = request.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        try
        {
            var userRepository = unitOfWork.Repository<User>();
            var createdUser = await userRepository.AddAsync(user);

            await unitOfWork.CommitAsync();
            return createdUser.Id > 0;
        }
        catch
        {
            unitOfWork.Rollback();
            throw;
        }
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var userRepository = unitOfWork.Repository<User>();

        var filters = new Dictionary<string, object>
        {
            { "Username", request.Username }
        };

        var users = await userRepository.GetByFilterAsync(filters);
        var user = users.FirstOrDefault();

        if (user == null) return null;

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        return !isPasswordValid ? null : GenerateJwtToken(user);
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(configuration["Jwt:Key"]!);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, ((UserType)user.UserTypeId).ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(5),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}