using System.ComponentModel.DataAnnotations;

namespace StourbridgeFc.Lucky7.Data.Models;

public class ReferralVisitSource
{
    public int Id { get; set; }
    
    [MaxLength(50)]
    public string Name { get; set; } = null!;
}