using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using StourbridgeFc.Lucky7.Api.DataTransferObjects;
using StourbridgeFc.Lucky7.Api.Services;

namespace StourbridgeFc.Lucky7.Api.Controllers;

[Route("registered-users")]
[ApiController]
public class RegisteredUsersController : ControllerBase
{
    private readonly RegisteredUsersService _registeredUsersService;

    public RegisteredUsersController(RegisteredUsersService registeredUsersService)
    {
        _registeredUsersService = registeredUsersService;
    }
    
    [HttpGet("is-entry-confirmed")]
    public async Task<ActionResult> IsEntryConfirmed([FromQuery(Name = "code")] string verificationCode)
    {
        bool isEntryConfirmed = await _registeredUsersService.IsUserEntryConfirmedAsync(verificationCode);
        return isEntryConfirmed ? NoContent() : NotFound();
    }
    
    [HttpGet("is-valid")]
    public async Task<ActionResult> IsVerificationCodeValid([FromQuery(Name = "code")] string verificationCode)
    {
        bool isVerificationCodeValid = await _registeredUsersService.IsUserVerificationCodeValidAsync(verificationCode);
        return isVerificationCodeValid ? NoContent() : NotFound();
    }
    
    [HttpGet("verify")]
    public async Task<IActionResult> VerifyUser([FromQuery(Name = "code")] string verificationCode)
    {
        bool isUserAlreadyVerified = await _registeredUsersService.IsUserVerifiedAsync(verificationCode);

        if (isUserAlreadyVerified)
            return Ok();
        
        bool userVerified = await _registeredUsersService.VerifyUserAsync(verificationCode);

        if (userVerified)
            await _registeredUsersService.SendUserVerifiedEmailAsync(verificationCode);
        
        return userVerified ? Ok() : BadRequest();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterUser(RegisterUserDto registerUser)
    {
        bool userRegistered = await _registeredUsersService.RegisterUserAsync(registerUser);
        return userRegistered ? Created() : BadRequest();
    }

    [HttpPost("confirm-entry")]
    public async Task<IActionResult> ConfirmEntry(ConfirmEntryDto confirmEntry)
    {
        bool isUserVerified = await _registeredUsersService.IsUserVerificationCodeValidAsync(confirmEntry.VerificationCode);
        
        if (isUserVerified is false)
            return NotFound("No verified user found");

        bool entryConfirmed = await _registeredUsersService.ConfirmEntryAsync(confirmEntry.VerificationCode);

        if (entryConfirmed is false)
            return Conflict("Entry already confirmed");
        
        await _registeredUsersService.SendEntryConfirmedEmailsAsync(confirmEntry.VerificationCode);
        return NoContent();
    }
}