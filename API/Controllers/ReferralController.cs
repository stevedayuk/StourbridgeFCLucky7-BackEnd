using Microsoft.AspNetCore.Mvc;
using StourbridgeFc.Lucky7.Api.DataTransferObjects;
using StourbridgeFc.Lucky7.Api.Services;

namespace StourbridgeFc.Lucky7.Api.Controllers;

[Route("referrals")]
[ApiController]
public class ReferralController : ControllerBase
{
    private readonly ReferralService _referralService;

    public ReferralController(ReferralService referralService)
    {
        _referralService = referralService;
    }

    [HttpPost]
    public async Task<IActionResult> SaveReferralVisitAsync(SaveReferralVisitDto saveReferralVisitDto)
    {
        await _referralService.SaveReferralVisitAsync(saveReferralVisitDto.Source);
        return NoContent();
    }
}