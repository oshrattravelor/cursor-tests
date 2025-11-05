using Adapter.Amadeus.Models.FlightSearch;

namespace Adapter.Amadeus.Services;

public interface IAmadeusFlightSearchService
{
    Task<FlightSearchResponse> SearchFlightsAsync(FlightSearchRequest request, CancellationToken cancellationToken = default);
}

