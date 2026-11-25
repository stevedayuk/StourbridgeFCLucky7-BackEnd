using Microsoft.EntityFrameworkCore;
using StourbridgeFc.Lucky7.Api.DataTransferObjects;
using StourbridgeFc.Lucky7.Data;

namespace StourbridgeFc.Lucky7.Api.Services;

public class WinnersService
{
    private readonly Lucky7Context _dataContext;

    public WinnersService(Lucky7Context dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<PublicCurrentDrawDto> GetCurrentDrawWinnersAsync()
    {
        PublicCurrentDrawDto currentDrawWinners = new();

        PublicDrawDto? currentDraw = await _dataContext.Draws.Where(p => p.IsPublished).Select(p => new PublicDrawDto
        {
            Month = p.Month,
            Year = p.Year,
        }).OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).FirstOrDefaultAsync();

        if (currentDraw is null)
        {
            return currentDrawWinners;
        }
        
        currentDrawWinners.Draw = currentDraw;

        List<PublicWinnerDto> winners = await GetDisplayWinnersAsync(currentDraw.Month, currentDraw.Year);
        currentDrawWinners.Winners = winners;
        
        return currentDrawWinners;
    }

    public async Task<List<PublicDrawDto>> GetPublishedDrawsListAsync()
    {
        List<PublicDrawDto> publishedDraws = await _dataContext.Draws.Where(p => p.IsPublished).Select(p => new PublicDrawDto
        {
            Month = p.Month,
            Year = p.Year,
        }).ToListAsync();

        return publishedDraws;
    }

    public async Task<List<PublicWinnerDto>> GetDisplayWinnersAsync(int month, int year)
    {
        var x = await _dataContext.DrawWinners
            .Include(e => e.Entry)
            .Include(e => e.PrizeLevel)
            // .Where(e => e.Draw.Month == month && e.Draw.Year == year)
            .ToListAsync();
        
        List<PublicWinnerDto> currentDrawWinners = await _dataContext.DrawWinners
            .Include(e => e.Entry)
            .Include(e => e.PrizeLevel)
            .Where(e => e.Draw.Month == month && e.Draw.Year == year)
            .OrderByDescending(p => p.PrizeLevel.Amount).ThenByDescending(p => p.Id)
            .Select(p => new PublicWinnerDto
            {
                PrizeAmount = p.PrizeLevel.Amount,
                Number = p.Entry.Number,
                Name = p.Entry.Name
            }).ToListAsync();
        
        return currentDrawWinners;
    }
}