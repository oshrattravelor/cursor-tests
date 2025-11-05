using System.Text.Json.Serialization;

namespace Adapter.Amadeus.Models.FlightSearch;

public class FlightSearchResponse
{
    [JsonPropertyName("meta")]
    public Meta? Meta { get; set; }
    
    [JsonPropertyName("data")]
    public List<FlightOffer> Data { get; set; } = new();
    
    [JsonPropertyName("dictionaries")]
    public Dictionaries? Dictionaries { get; set; }
}

public class Meta
{
    [JsonPropertyName("count")]
    public int Count { get; set; }
    
    [JsonPropertyName("links")]
    public Links? Links { get; set; }
}

public class Links
{
    [JsonPropertyName("self")]
    public string? Self { get; set; }
}

public class Dictionaries
{
    [JsonPropertyName("locations")]
    public Dictionary<string, Location>? Locations { get; set; }
    
    [JsonPropertyName("aircraft")]
    public Dictionary<string, string>? Aircraft { get; set; }
    
    [JsonPropertyName("currencies")]
    public Dictionary<string, string>? Currencies { get; set; }
    
    [JsonPropertyName("carriers")]
    public Dictionary<string, string>? Carriers { get; set; }
}

public class Location
{
    [JsonPropertyName("cityCode")]
    public string? CityCode { get; set; }
    
    [JsonPropertyName("countryCode")]
    public string? CountryCode { get; set; }
}

public class FlightOffer
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
    
    [JsonPropertyName("instantTicketingRequired")]
    public bool InstantTicketingRequired { get; set; }
    
    [JsonPropertyName("nonHomogeneous")]
    public bool NonHomogeneous { get; set; }
    
    [JsonPropertyName("oneWay")]
    public bool OneWay { get; set; }
    
    [JsonPropertyName("lastTicketingDate")]
    public string? LastTicketingDate { get; set; }
    
    [JsonPropertyName("numberOfBookableSeats")]
    public int NumberOfBookableSeats { get; set; }
    
    [JsonPropertyName("itineraries")]
    public List<Itinerary> Itineraries { get; set; } = new();
    
    [JsonPropertyName("price")]
    public Price? Price { get; set; }
    
    [JsonPropertyName("pricingOptions")]
    public PricingOptions? PricingOptions { get; set; }
    
    [JsonPropertyName("validatingAirlineCodes")]
    public List<string> ValidatingAirlineCodes { get; set; } = new();
    
    [JsonPropertyName("travelerPricings")]
    public List<TravelerPricing> TravelerPricings { get; set; } = new();
}

public class Itinerary
{
    [JsonPropertyName("duration")]
    public string? Duration { get; set; }
    
    [JsonPropertyName("segments")]
    public List<Segment> Segments { get; set; } = new();
}

public class Segment
{
    [JsonPropertyName("departure")]
    public FlightEndPoint? Departure { get; set; }
    
    [JsonPropertyName("arrival")]
    public FlightEndPoint? Arrival { get; set; }
    
    [JsonPropertyName("carrierCode")]
    public string CarrierCode { get; set; } = string.Empty;
    
    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;
    
    [JsonPropertyName("aircraft")]
    public Aircraft? Aircraft { get; set; }
    
    [JsonPropertyName("operating")]
    public Operating? Operating { get; set; }
    
    [JsonPropertyName("duration")]
    public string? Duration { get; set; }
    
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("numberOfStops")]
    public int NumberOfStops { get; set; }
    
    [JsonPropertyName("blacklistedInEU")]
    public bool BlacklistedInEU { get; set; }
}

public class FlightEndPoint
{
    [JsonPropertyName("iataCode")]
    public string IataCode { get; set; } = string.Empty;
    
    [JsonPropertyName("terminal")]
    public string? Terminal { get; set; }
    
    [JsonPropertyName("at")]
    public DateTime At { get; set; }
}

public class Aircraft
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;
}

public class Operating
{
    [JsonPropertyName("carrierCode")]
    public string CarrierCode { get; set; } = string.Empty;
}

public class Price
{
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    
    [JsonPropertyName("total")]
    public string Total { get; set; } = string.Empty;
    
    [JsonPropertyName("base")]
    public string Base { get; set; } = string.Empty;
    
    [JsonPropertyName("fees")]
    public List<Fee> Fees { get; set; } = new();
    
    [JsonPropertyName("grandTotal")]
    public string GrandTotal { get; set; } = string.Empty;
}

public class Fee
{
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class PricingOptions
{
    [JsonPropertyName("fareType")]
    public List<string> FareType { get; set; } = new();
    
    [JsonPropertyName("includedCheckedBagsOnly")]
    public bool IncludedCheckedBagsOnly { get; set; }
}

public class TravelerPricing
{
    [JsonPropertyName("travelerId")]
    public string TravelerId { get; set; } = string.Empty;
    
    [JsonPropertyName("fareOption")]
    public string FareOption { get; set; } = string.Empty;
    
    [JsonPropertyName("travelerType")]
    public string TravelerType { get; set; } = string.Empty;
    
    [JsonPropertyName("price")]
    public Price? Price { get; set; }
    
    [JsonPropertyName("fareDetailsBySegment")]
    public List<FareDetailsBySegment> FareDetailsBySegment { get; set; } = new();
}

public class FareDetailsBySegment
{
    [JsonPropertyName("segmentId")]
    public string SegmentId { get; set; } = string.Empty;
    
    [JsonPropertyName("cabin")]
    public string Cabin { get; set; } = string.Empty;
    
    [JsonPropertyName("fareBasis")]
    public string FareBasis { get; set; } = string.Empty;
    
    [JsonPropertyName("class")]
    public string Class { get; set; } = string.Empty;
    
    [JsonPropertyName("includedCheckedBags")]
    public IncludedCheckedBags? IncludedCheckedBags { get; set; }
}

public class IncludedCheckedBags
{
    [JsonPropertyName("weight")]
    public int? Weight { get; set; }
    
    [JsonPropertyName("weightUnit")]
    public string? WeightUnit { get; set; }
    
    [JsonPropertyName("quantity")]
    public int? Quantity { get; set; }
}

