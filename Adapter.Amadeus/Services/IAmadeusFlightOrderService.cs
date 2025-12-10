using Adapter.Amadeus.Models.FlightOrder;

namespace Adapter.Amadeus.Services;

/// <summary>
/// Service interface for creating flight orders with Amadeus API
/// </summary>
public interface IAmadeusFlightOrderService
{
    /// <summary>
    /// Create a flight order (booking) using confirmed flight offers and traveler information
    /// </summary>
    /// <param name="request">Flight order request containing flight offers, travelers, and contact information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flight order response with booking confirmation</returns>
    Task<FlightOrderResponse> CreateFlightOrderAsync(
        FlightOrderRequest request,
        CancellationToken cancellationToken = default);
}

