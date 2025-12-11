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

    /// <summary>
    /// Issue tickets for an existing flight order
    /// </summary>
    /// <param name="orderId">The ID of the flight order to issue tickets for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flight order response with issued ticket information</returns>
    Task<FlightOrderResponse> IssueFlightOrderAsync(
        string orderId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a TST (Transitional Stored Ticket) for an existing flight order
    /// TST is typically created automatically during order creation, but this method allows explicit TST creation
    /// </summary>
    /// <param name="orderId">The ID of the flight order to create TST for</param>
    /// <param name="travelerIds">Optional list of traveler IDs to include in TST. If null, includes all travelers.</param>
    /// <param name="segmentIds">Optional list of segment IDs to include in TST. If null, includes all segments.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flight order response with TST information</returns>
    Task<FlightOrderResponse> CreateTSTAsync(
        string orderId,
        List<string>? travelerIds = null,
        List<string>? segmentIds = null,
        CancellationToken cancellationToken = default);
}

