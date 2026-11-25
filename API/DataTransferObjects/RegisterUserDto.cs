namespace StourbridgeFc.Lucky7.Api.DataTransferObjects;

public class RegisterUserDto : RegisteredUserDto
{
    public string TurnstileToken { get; set; } = null!;
}