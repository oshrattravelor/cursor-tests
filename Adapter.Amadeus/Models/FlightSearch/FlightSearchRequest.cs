namespace Adapter.Amadeus.Models.FlightSearch;

public class FlightSearchRequest
{
    public string OriginLocationCode { get; set; } = string.Empty;
    public string DestinationLocationCode { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int Adults { get; set; } = 1;
    public int? Children { get; set; }
    public int? Infants { get; set; }
    public string? TravelClass { get; set; } // ECONOMY, PREMIUM_ECONOMY, BUSINESS, FIRST
    public bool NonStop { get; set; } = false;
    public string CurrencyCode { get; set; } = "USD";
    public int MaxResults { get; set; } = 10;
}

