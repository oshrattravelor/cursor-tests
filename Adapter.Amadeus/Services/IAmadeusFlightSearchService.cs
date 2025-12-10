using Adapter.Amadeus.Models.FlightSearch;

namespace Adapter.Amadeus.Services;

public interface IAmadeusFlightSearchService
{
    Task<FlightSearchResponse> SearchFlightsAsync(FlightSearchRequest request, CancellationToken cancellationToken = default);
    
    Task<FlightOffersPricingResponse> PriceFlightOffersAsync(
        List<FlightOffer> flightOffers, 
        CancellationToken cancellationToken = default);
}

