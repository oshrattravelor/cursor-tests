namespace Adapter.Amadeus.Models.FlightSearch;

/// <summary>
/// Request model for flight search supporting one-way, round-trip, and multi-city flights
/// </summary>
public class FlightSearchRequest
{
    /// <summary>
    /// Type of trip: OneWay, RoundTrip, or MultiCity
    /// If not specified, will be inferred from the request data
    /// </summary>
    public TripType? TripType { get; set; }
    
    // Properties for OneWay and RoundTrip flights
    /// <summary>
    /// Origin airport code (IATA code) - Required for OneWay and RoundTrip
    /// </summary>
    public string? OriginLocationCode { get; set; }
    
    /// <summary>
    /// Destination airport code (IATA code) - Required for OneWay and RoundTrip
    /// </summary>
    public string? DestinationLocationCode { get; set; }
    
    /// <summary>
    /// Departure date - Required for OneWay and RoundTrip
    /// </summary>
    public DateTime? DepartureDate { get; set; }
    
    /// <summary>
    /// Return date - Required for RoundTrip, ignored for OneWay
    /// </summary>
    public DateTime? ReturnDate { get; set; }
    
    // Properties for MultiCity flights
    /// <summary>
    /// List of flight segments for multi-city trips - Required for MultiCity
    /// </summary>
    public List<FlightSegment>? Segments { get; set; }
    
    // Common properties for all trip types
    /// <summary>
    /// Number of adult passengers (default: 1)
    /// </summary>
    public int Adults { get; set; } = 1;
    
    /// <summary>
    /// Number of child passengers
    /// </summary>
    public int? Children { get; set; }
    
    /// <summary>
    /// Number of infant passengers
    /// </summary>
    public int? Infants { get; set; }
    
    /// <summary>
    /// Travel class: ECONOMY, PREMIUM_ECONOMY, BUSINESS, FIRST
    /// </summary>
    public string? TravelClass { get; set; }
    
    /// <summary>
    /// Whether to search for non-stop flights only (default: false)
    /// </summary>
    public bool NonStop { get; set; } = false;
    
    /// <summary>
    /// Currency code for pricing (default: USD)
    /// </summary>
    public string CurrencyCode { get; set; } = "USD";
    
    /// <summary>
    /// Maximum number of results to return (default: 10)
    /// </summary>
    public int MaxResults { get; set; } = 10;
    
    /// <summary>
    /// Whether to include branded fare information in the response (default: false)
    /// When enabled, the response will include branded fare options with additional services
    /// </summary>
    public bool IncludeBrandedFares { get; set; } = false;
    
    /// <summary>
    /// Determines the trip type based on the request data if not explicitly set
    /// </summary>
    public TripType GetTripType()
    {
        if (TripType.HasValue)
        {
            return TripType.Value;
        }
        
        // Infer trip type from data
        if (Segments != null && Segments.Count > 0)
        {
            return Models.FlightSearch.TripType.MultiCity;
        }
        
        if (ReturnDate.HasValue)
        {
            return Models.FlightSearch.TripType.RoundTrip;
        }
        
        return Models.FlightSearch.TripType.OneWay;
    }
}

