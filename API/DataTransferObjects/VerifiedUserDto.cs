namespace StourbridgeFc.Lucky7.Api.DataTransferObjects;

public class VerifiedUserDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string VerificationCode { get; set; }
}