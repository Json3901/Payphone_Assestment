using Microsoft.AspNetCore.Mvc;
using Payphone_Assestment.Application.Dtos.User;
using Payphone_Assestment.Application.Interfaces;

namespace Payphone_Assestment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Username and Password are required.");

        var userId = await userService.RegisterAsync(request, request.Password);
        return Ok(new { userId });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await userService.LoginAsync(request);

        if (token == null)
            return Unauthorized(new { message = "Invalid credentials." });

        return Ok(new { token });
    }
}