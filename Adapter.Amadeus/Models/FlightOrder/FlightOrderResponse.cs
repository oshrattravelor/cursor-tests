using System.Text.Json.Serialization;
using Adapter.Amadeus.Models.FlightSearch;

namespace Adapter.Amadeus.Models.FlightOrder;

/// <summary>
/// Response model for flight order creation
/// </summary>
public class FlightOrderResponse
{
    [JsonPropertyName("data")]
    public FlightOrderResponseData? Data { get; set; }

    [JsonPropertyName("warnings")]
    public List<Warning>? Warnings { get; set; }
}

/// <summary>
/// Data container for flight order response
/// </summary>
public class FlightOrderResponseData
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("queuingOfficeId")]
    public string? QueuingOfficeId { get; set; }

    [JsonPropertyName("associatedRecords")]
    public List<AssociatedRecord>? AssociatedRecords { get; set; }

    [JsonPropertyName("flightOffers")]
    public List<FlightOffer> FlightOffers { get; set; } = new();

    [JsonPropertyName("travelers")]
    public List<TravelerInfo> Travelers { get; set; } = new();

    [JsonPropertyName("ticketingAgreement")]
    public TicketingAgreement? TicketingAgreement { get; set; }

    [JsonPropertyName("contacts")]
    public List<Contact>? Contacts { get; set; }
}

/// <summary>
/// Associated record (PNR, etc.)
/// </summary>
public class AssociatedRecord
{
    [JsonPropertyName("reference")]
    public string Reference { get; set; } = string.Empty;

    [JsonPropertyName("creationDate")]
    public string? CreationDate { get; set; }

    [JsonPropertyName("originSystemCode")]
    public string OriginSystemCode { get; set; } = string.Empty;

    [JsonPropertyName("flightOfferId")]
    public string? FlightOfferId { get; set; }
}

