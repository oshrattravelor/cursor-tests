using System.Text.Json.Serialization;

namespace Adapter.Amadeus.Models.FlightSearch;

/// <summary>
/// Request model for Flight Offers Price endpoint
/// Used to confirm pricing and get detailed fare rules for flight offers
/// </summary>
public class FlightOffersPricingRequest
{
    /// <summary>
    /// The flight offers from a search that need to be priced
    /// </summary>
    [JsonPropertyName("data")]
    public FlightOffersPricingData Data { get; set; } = new();
}

/// <summary>
/// Data container for flight offers pricing request
/// </summary>
public class FlightOffersPricingData
{
    /// <summary>
    /// Type of the pricing request
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "flight-offers-pricing";

    /// <summary>
    /// The flight offers to price
    /// </summary>
    [JsonPropertyName("flightOffers")]
    public List<FlightOffer> FlightOffers { get; set; } = new();
}

