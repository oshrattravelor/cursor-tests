namespace Adapter.Amadeus.Models.FlightSearch;

/// <summary>
/// Represents the type of flight trip
/// </summary>
public enum TripType
{
    /// <summary>
    /// One-way flight (single leg)
    /// </summary>
    OneWay,
    
    /// <summary>
    /// Round-trip flight (outbound and return)
    /// </summary>
    RoundTrip,
    
    /// <summary>
    /// Multi-city flight (multiple destinations)
    /// </summary>
    MultiCity
}

