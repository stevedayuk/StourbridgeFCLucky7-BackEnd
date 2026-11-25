using StourbridgeFc.Lucky7.Data;
using StourbridgeFc.Lucky7.Data.Models;

namespace StourbridgeFc.Lucky7.Api.Services;

public class ReferralService
{
    private readonly Lucky7Context _dataContext;

    public ReferralService(Lucky7Context dataContext)
    {
        _dataContext = dataContext;
    }
    
    public async Task SaveReferralVisitAsync(string source)
    {
        int sourceId = source.ToLower() switch
        {
            "social" => 2,
            "qrcode" => 3,
            _ => 1
        };

        var referralVisit = new ReferralVisit
        {
            SourceId = sourceId,
            VisitDate = DateTime.UtcNow
        };

        _dataContext.ReferralVisits.Add(referralVisit);
        await _dataContext.SaveChangesAsync();
    }
}