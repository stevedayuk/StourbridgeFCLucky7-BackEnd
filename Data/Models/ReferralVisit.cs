namespace StourbridgeFc.Lucky7.Data.Models;

public class ReferralVisit
{
    public int Id { get; set; }
    public int SourceId { get; set; }
    public DateTime VisitDate { get; set; }
    
    public ReferralVisitSource ReferralVisitSource { get; set; } = null!;
}