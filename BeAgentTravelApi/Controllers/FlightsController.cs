using System.Net.Http;
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
    /// Supports one-way, round-trip, and multi-city flights
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
            // Validate request
            var validationError = ValidateRequest(request);
            if (validationError != null)
            {
                return BadRequest(new { error = validationError });
            }

            var tripType = request.GetTripType();
            
            _logger.LogInformation(
                "Searching {TripType} flights - Adults: {Adults}, Children: {Children}, Infants: {Infants}",
                tripType,
                request.Adults,
                request.Children ?? 0,
                request.Infants ?? 0);

            if (tripType == TripType.MultiCity && request.Segments != null)
            {
                _logger.LogInformation(
                    "Multi-city search with {SegmentCount} segments",
                    request.Segments.Count);
                foreach (var segment in request.Segments)
                {
                    _logger.LogInformation(
                        "Segment: {Origin} -> {Destination} on {Date}",
                        segment.OriginLocationCode,
                        segment.DestinationLocationCode,
                        segment.DepartureDate);
                }
            }
            else
            {
                _logger.LogInformation(
                    "Searching flights from {Origin} to {Destination} on {DepartureDate}",
                    request.OriginLocationCode,
                    request.DestinationLocationCode,
                    request.DepartureDate);
                
                if (tripType == TripType.RoundTrip && request.ReturnDate.HasValue)
                {
                    _logger.LogInformation("Return date: {ReturnDate}", request.ReturnDate.Value);
                }
            }

            var result = await _flightSearchService.SearchFlightsAsync(request, cancellationToken);

            _logger.LogInformation("Found {Count} flight offers", result.Data?.Count ?? 0);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters");
            return BadRequest(new { error = ex.Message });
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

    private string? ValidateRequest(FlightSearchRequest request)
    {
        var tripType = request.GetTripType();

        if (tripType == TripType.MultiCity)
        {
            if (request.Segments == null || request.Segments.Count == 0)
            {
                return "Segments are required for multi-city flights";
            }
            
            if (request.Segments.Count < 2)
            {
                return "Multi-city flights require at least 2 segments";
            }
            
            foreach (var segment in request.Segments)
            {
                if (string.IsNullOrWhiteSpace(segment.OriginLocationCode))
                {
                    return "OriginLocationCode is required for all segments";
                }
                
                if (string.IsNullOrWhiteSpace(segment.DestinationLocationCode))
                {
                    return "DestinationLocationCode is required for all segments";
                }
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.OriginLocationCode))
            {
                return "OriginLocationCode is required";
            }
            
            if (string.IsNullOrWhiteSpace(request.DestinationLocationCode))
            {
                return "DestinationLocationCode is required";
            }
            
            if (!request.DepartureDate.HasValue)
            {
                return "DepartureDate is required";
            }
            
            if (tripType == TripType.RoundTrip && !request.ReturnDate.HasValue)
            {
                return "ReturnDate is required for round-trip flights";
            }
            
            if (request.ReturnDate.HasValue && request.ReturnDate.Value <= request.DepartureDate.Value)
            {
                return "ReturnDate must be after DepartureDate";
            }
        }

        if (request.Adults < 1)
        {
            return "At least one adult passenger is required";
        }

        return null;
    }

    /// <summary>
    /// Get sample flight search requests for testing (one-way, round-trip, and multi-city)
    /// </summary>
    [HttpGet("sample-request")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetSampleRequest([FromQuery] string? tripType = "roundtrip")
    {
        var tripTypeLower = tripType?.ToLowerInvariant();
        
        if (tripTypeLower == "oneway" || tripTypeLower == "one-way")
        {
            var sample = new FlightSearchRequest
            {
                TripType = TripType.OneWay,
                OriginLocationCode = "NYC",
                DestinationLocationCode = "LAX",
                DepartureDate = DateTime.Today.AddDays(30),
                Adults = 1,
                TravelClass = "ECONOMY",
                NonStop = false,
                CurrencyCode = "USD",
                MaxResults = 10
            };
            return Ok(new { tripType = "OneWay", sample });
        }
        else if (tripTypeLower == "multicity" || tripTypeLower == "multi-city")
        {
            var sample = new FlightSearchRequest
            {
                TripType = TripType.MultiCity,
                Segments = new List<FlightSegment>
                {
                    new FlightSegment
                    {
                        OriginLocationCode = "NYC",
                        DestinationLocationCode = "LAX",
                        DepartureDate = DateTime.Today.AddDays(30)
                    },
                    new FlightSegment
                    {
                        OriginLocationCode = "LAX",
                        DestinationLocationCode = "SFO",
                        DepartureDate = DateTime.Today.AddDays(35)
                    },
                    new FlightSegment
                    {
                        OriginLocationCode = "SFO",
                        DestinationLocationCode = "NYC",
                        DepartureDate = DateTime.Today.AddDays(40)
                    }
                },
                Adults = 1,
                TravelClass = "ECONOMY",
                CurrencyCode = "USD",
                MaxResults = 10
            };
            return Ok(new { tripType = "MultiCity", sample });
        }
        else
        {
            // Default: Round-trip
            var sample = new FlightSearchRequest
            {
                TripType = TripType.RoundTrip,
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
            return Ok(new { tripType = "RoundTrip", sample });
        }
    }

    /// <summary>
    /// Get confirmed pricing and detailed fare rules for flight offers
    /// This endpoint should be called after a flight search to get final pricing and fare conditions.
    /// The request automatically includes detailed fare rules (include=detailed-fare-rules parameter).
    /// </summary>
    /// <param name="request">Request containing flight offers to price</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flight offers with confirmed pricing and detailed fare rules</returns>
    [HttpPost("price")]
    [ProducesResponseType(typeof(FlightOffersPricingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PriceFlightOffers(
        [FromBody] FlightOffersPricingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate request
            if (request?.Data?.FlightOffers == null || request.Data.FlightOffers.Count == 0)
            {
                return BadRequest(new { error = "At least one flight offer is required for pricing" });
            }

            _logger.LogInformation(
                "Pricing {Count} flight offers",
                request.Data.FlightOffers.Count);

            var result = await _flightSearchService.PriceFlightOffersAsync(
                request.Data.FlightOffers, 
                cancellationToken);

            _logger.LogInformation(
                "Pricing completed. Returned {Count} priced offers",
                result.Data?.FlightOffers?.Count ?? 0);

            if (result.Warnings != null && result.Warnings.Count > 0)
            {
                _logger.LogWarning(
                    "Pricing completed with {WarningCount} warnings",
                    result.Warnings.Count);
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters for pricing");
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while pricing flight offers");
            return StatusCode(StatusCodes.Status502BadGateway, 
                new { error = "Failed to communicate with Amadeus API", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while pricing flight offers");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { error = "An unexpected error occurred", details = ex.Message });
        }
    }
}

