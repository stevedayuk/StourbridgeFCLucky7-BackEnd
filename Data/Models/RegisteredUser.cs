namespace StourbridgeFc.Lucky7.Data.Models;

public class RegisteredUser
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string Town { get; set; }
    public string County { get; set; }
    public string Postcode { get; set; }
    public string? Telephone { get; set; }
    public string Email { get; set; }
    public DateTime RegisteredDate { get; set; }
    public string? VerificationCode { get; set; }
    public DateTime? VerifiedDate { get; set; }
    public DateTime? ConfirmedDate { get; set; }
}