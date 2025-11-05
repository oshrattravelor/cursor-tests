using System.Text.Json.Serialization;

namespace Adapter.Amadeus.Models.Authentication;

public class AccessTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("state")]
    public string? State { get; set; }
    
    public DateTime ExpiresAt { get; set; }
}

