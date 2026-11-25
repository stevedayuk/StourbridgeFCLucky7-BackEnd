using System.Text.Json.Serialization;

namespace StourbridgeFc.Lucky7.Api.DataTransferObjects;

public class TurnstileResponseDto
{
    public string? Action { get; set; }
    public string? CData { get; set; }
    
    [JsonPropertyName("challenge_ts")]
    public DateTime ChallengeTime { get; set; }
    
    [JsonPropertyName("error_codes")]
    public object[]? ErrorCodes { get; set; }

    public string? HostName { get; set; }
    
    public TurnstileResponseMetadataDto? Metadata { get; set; }
    public bool Success { get; set; }
}