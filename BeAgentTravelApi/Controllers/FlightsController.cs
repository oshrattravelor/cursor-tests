using Adapter.Amadeus.Models.FlightSearch;
using Adapter.Amadeus.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeAgentTravelApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IAmadeusFlightSearchService _flightSearchService;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(
        IAmadeusFlightSearchService flightSearchService,
        ILogger<FlightsController> logger)
    {
        _flightSearchService = flightSearchService;
        _logger = logger;
    }

    /// <summary>
    /// Search for flights using Amadeus GDS
    /// </summary>
    /// <param name="request">Flight search parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of available flight offers</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(FlightSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchFlights(
        [FromBody] FlightSearchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Searching flights from {Origin} to {Destination} on {DepartureDate}",
                request.OriginLocationCode,
                request.DestinationLocationCode,
                request.DepartureDate);

            var result = await _flightSearchService.SearchFlightsAsync(request, cancellationToken);

            _logger.LogInformation("Found {Count} flight offers", result.Data.Count);

            return Ok(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while searching flights");
            return StatusCode(StatusCodes.Status502BadGateway, 
                new { error = "Failed to communicate with Amadeus API", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while searching flights");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An unexpected error occurred", details = ex.Message });
        }
    }

    /// <summary>
    /// Get a sample flight search request for testing
    /// </summary>
    [HttpGet("sample-request")]
    [ProducesResponseType(typeof(FlightSearchRequest), StatusCodes.Status200OK)]
    public IActionResult GetSampleRequest()
    {
        var sample = new FlightSearchRequest
        {
            OriginLocationCode = "NYC",
            DestinationLocationCode = "LAX",
            DepartureDate = DateTime.Today.AddDays(30),
            ReturnDate = DateTime.Today.AddDays(37),
            Adults = 1,
            Children = 0,
            Infants = 0,
            TravelClass = "ECONOMY",
            NonStop = false,
            CurrencyCode = "USD",
            MaxResults = 10
        };

        return Ok(sample);
    }
}

