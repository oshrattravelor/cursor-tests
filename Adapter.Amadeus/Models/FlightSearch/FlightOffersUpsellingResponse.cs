using System.Text.Json.Serialization;

namespace Adapter.Amadeus.Models.FlightSearch;

/// <summary>
/// Response model for Flight Offers Upselling endpoint
/// Contains enhanced flight offers with branded fare options and upsell opportunities
/// </summary>
public class FlightOffersUpsellingResponse
{
    [JsonPropertyName("data")]
    public FlightOffersUpsellingResponseData? Data { get; set; }

    [JsonPropertyName("warnings")]
    public List<Warning>? Warnings { get; set; }
}

/// <summary>
/// Data container for flight offers upselling response
/// </summary>
public class FlightOffersUpsellingResponseData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("flightOffers")]
    public List<UpselledFlightOffer> FlightOffers { get; set; } = new();
}

/// <summary>
/// Flight offer with upselling options and branded fare information
/// </summary>
public class UpselledFlightOffer : FlightOffer
{
    /// <summary>
    /// Branded fare information (e.g., "ECOLIGHT", "ECOFLEX", "ECOPLUS")
    /// </summary>
    [JsonPropertyName("brandedFare")]
    public string? BrandedFare { get; set; }

    /// <summary>
    /// Additional services available for upsell
    /// </summary>
    [JsonPropertyName("additionalServices")]
    public List<AdditionalService>? AdditionalServices { get; set; }
}

/// <summary>
/// Additional service available for upsell
/// </summary>
public class AdditionalService
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string? Amount { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

