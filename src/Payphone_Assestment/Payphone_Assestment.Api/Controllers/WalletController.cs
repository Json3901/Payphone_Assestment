using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payphone_Assestment.Application.Dtos.Wallet;
using Payphone_Assestment.Application.Interfaces;

namespace Payphone_Assestment.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class WalletController(IWalletService walletService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<IActionResult> Register([FromBody] CreateWalletRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Wallet name is required.");

        if (request.InitialBalance < 0)
            return BadRequest("Initial balance cannot be negative.");

        var created = await walletService.CreateAsync(request);

        return Ok(new { created });
    }

    [HttpGet("list")]
    public async Task<IActionResult> GetWalletByUser()
    {
        var wallets = await walletService.GetAllByUserAsync();
        return wallets.Any() ? Ok(wallets) : NotFound();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWalletByUser(int id)
    {
        var wallet = await walletService.GetByIdAsync(id);
        return wallet != null ? Ok(wallet) : NotFound();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWalletRequest request)
    {
        var success = await walletService.UpdateAsync(id, request);
        return success ? Ok() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await walletService.DeleteAsync(id);
        return success ? Ok() : NotFound();
    }
    
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(TransferRequest request)
    {
        try
        {
            await walletService.TransferAsync(request);
            return Ok("Transfer successful.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}