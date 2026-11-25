namespace StourbridgeFc.Lucky7.Api.DataTransferObjects;

public class PublicCurrentDrawDto
{
    public PublicDrawDto Draw { get; set; } = null!;
    public List<PublicWinnerDto> Winners { get; set; } = null!;
}