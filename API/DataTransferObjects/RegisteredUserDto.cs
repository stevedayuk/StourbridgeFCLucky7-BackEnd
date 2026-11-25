namespace StourbridgeFc.Lucky7.Api.DataTransferObjects;

public class RegisteredUserDto
{
    public string Name { get; set; } = null!;
    public string AddressLine1 { get; set; } = null!;
    public string? AddressLine2 { get; set; }
    public string Town { get; set; } = null!;
    public string County { get; set; } = null!;
    public string Postcode { get; set; } = null!;
    public string? Telephone { get; set; }
    public string Email { get; set; } = null!;
}