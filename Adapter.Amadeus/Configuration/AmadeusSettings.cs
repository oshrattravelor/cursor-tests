namespace Adapter.Amadeus.Configuration;

public class AmadeusSettings
{
    public const string SectionName = "Amadeus";
    
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://test.api.amadeus.com";
    public bool IsProduction { get; set; } = false;
}

