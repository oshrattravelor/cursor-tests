namespace Adapter.Amadeus.Models.FlightSearch;

/// <summary>
/// Represents a flight segment for multi-city trips
/// </summary>
public class FlightSegment
{
    /// <summary>
    /// Origin airport code (IATA code)
    /// </summary>
    public string OriginLocationCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Destination airport code (IATA code)
    /// </summary>
    public string DestinationLocationCode { get; set; } = string.Empty;
    
    /// <summary>
    /// Departure date for this segment
    /// </summary>
    public DateTime DepartureDate { get; set; }
}

