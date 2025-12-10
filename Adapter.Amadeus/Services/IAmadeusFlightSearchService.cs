using Adapter.Amadeus.Models.FlightSearch;

namespace Adapter.Amadeus.Services;

public interface IAmadeusFlightSearchService
{
    Task<FlightSearchResponse> SearchFlightsAsync(FlightSearchRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get confirmed pricing and detailed fare rules for flight offers
    /// The request automatically includes detailed fare rules via the include=detailed-fare-rules parameter
    /// </summary>
    /// <param name="flightOffers">Flight offers from a search to price</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flight offers with confirmed pricing and detailed fare rules</returns>
    Task<FlightOffersPricingResponse> PriceFlightOffersAsync(
        List<FlightOffer> flightOffers, 
        CancellationToken cancellationToken = default);
}

