using System.Text.Json.Serialization;

namespace Adapter.Amadeus.Models.FlightSearch;

/// <summary>
/// Request model for Flight Offers Upselling endpoint
/// Used to get branded fare options and upsell opportunities for flight offers
/// </summary>
public class FlightOffersUpsellingRequest
{
    /// <summary>
    /// The flight offers to get upselling options for
    /// </summary>
    [JsonPropertyName("data")]
    public FlightOffersUpsellingData Data { get; set; } = new();
}

/// <summary>
/// Data container for flight offers upselling request
/// </summary>
public class FlightOffersUpsellingData
{
    /// <summary>
    /// Type of the upselling request
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "flight-offers-upselling";

    /// <summary>
    /// The flight offers to get upselling options for
    /// </summary>
    [JsonPropertyName("flightOffers")]
    public List<FlightOffer> FlightOffers { get; set; } = new();
}

