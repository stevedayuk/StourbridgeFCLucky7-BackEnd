using Microsoft.AspNetCore.Mvc;
using StourbridgeFc.Lucky7.Api.DataTransferObjects;
using StourbridgeFc.Lucky7.Api.Services;

namespace StourbridgeFc.Lucky7.Api.Controllers;

[Route("public")]
[ApiController]
public class PublicController : ControllerBase
{
    private readonly WinnersService _winnersService;

    public PublicController(WinnersService winnersService)
    {
        _winnersService = winnersService;
    }
    
    [HttpGet("current-draw")]
    public async Task<ActionResult<PublicCurrentDrawDto>> GetCurrentDrawWinnersAsync()
    {
        PublicCurrentDrawDto publicWinners = await _winnersService.GetCurrentDrawWinnersAsync();
        return publicWinners;
    }
    
    [HttpGet("draws")]
    public async Task<ActionResult<List<PublicDrawDto>>> GetPublishedDrawsListAsync()
    {
        List<PublicDrawDto> publishedDraws = await _winnersService.GetPublishedDrawsListAsync();
        return publishedDraws;
    }

    [HttpGet("draw")]
    public async Task<ActionResult<List<PublicWinnerDto>>> GetDrawDisplayWinners(int month, int year)
    {
        List<PublicWinnerDto> publicWinners = await _winnersService.GetDisplayWinnersAsync(month, year);
        return publicWinners;   
    }
}