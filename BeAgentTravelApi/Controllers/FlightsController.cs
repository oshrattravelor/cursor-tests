using System.Net.Http;
using Adapter.Amadeus.Models.FlightOrder;
using Adapter.Amadeus.Models.FlightSearch;
using Adapter.Amadeus.Services;
using Microsoft.AspNetCore.Mvc;

namespace BeAgentTravelApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IAmadeusFlightSearchService _flightSearchService;
    private readonly IAmadeusFlightOrderService _flightOrderService;
    private readonly ILogger<FlightsController> _logger;

    public FlightsController(
        IAmadeusFlightSearchService flightSearchService,
        IAmadeusFlightOrderService flightOrderService,
        ILogger<FlightsController> logger)
    {
        _flightSearchService = flightSearchService;
        _flightOrderService = flightOrderService;
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

    /// <summary>
    /// Create a flight order (booking) using confirmed flight offers and traveler information
    /// This endpoint should be called after pricing flight offers to create a booking.
    /// </summary>
    /// <param name="request">Flight order request containing flight offers, travelers, and contact information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flight order response with booking confirmation</returns>
    [HttpPost("order")]
    [ProducesResponseType(typeof(FlightOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateFlightOrder(
        [FromBody] FlightOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate request
            if (request?.Data == null)
            {
                return BadRequest(new { error = "Request data is required" });
            }

            if (request.Data.FlightOffers == null || request.Data.FlightOffers.Count == 0)
            {
                return BadRequest(new { error = "At least one flight offer is required" });
            }

            if (request.Data.Travelers == null || request.Data.Travelers.Count == 0)
            {
                return BadRequest(new { error = "At least one traveler is required" });
            }

            _logger.LogInformation(
                "Creating flight order for {TravelerCount} travelers with {OfferCount} flight offers",
                request.Data.Travelers.Count,
                request.Data.FlightOffers.Count);

            var result = await _flightOrderService.CreateFlightOrderAsync(request, cancellationToken);

            _logger.LogInformation(
                "Flight order created successfully. Order ID: {OrderId}",
                result.Data?.Id ?? "Unknown");

            if (result.Warnings != null && result.Warnings.Count > 0)
            {
                _logger.LogWarning(
                    "Flight order created with {WarningCount} warnings",
                    result.Warnings.Count);
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters for flight order");
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while creating flight order");
            return StatusCode(StatusCodes.Status502BadGateway,
                new { error = "Failed to communicate with Amadeus API", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while creating flight order");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred", details = ex.Message });
        }
    }

    /// <summary>
    /// Issue tickets for an existing flight order
    /// This endpoint should be called after creating a flight order to issue the tickets.
    /// Note: Some orders may have tickets issued automatically during creation if payment is provided.
    /// </summary>
    /// <param name="orderId">The ID of the flight order to issue tickets for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Flight order response with issued ticket information</returns>
    [HttpPatch("order/{orderId}/issue")]
    [ProducesResponseType(typeof(FlightOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IssueFlightOrder(
        [FromRoute] string orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return BadRequest(new { error = "Order ID is required" });
            }

            _logger.LogInformation(
                "Issuing tickets for flight order: {OrderId}",
                orderId);

            var result = await _flightOrderService.IssueFlightOrderAsync(orderId, cancellationToken);

            _logger.LogInformation(
                "Tickets issued successfully for order ID: {OrderId}",
                orderId);

            if (result.Warnings != null && result.Warnings.Count > 0)
            {
                _logger.LogWarning(
                    "Ticket issuance completed with {WarningCount} warnings",
                    result.Warnings.Count);
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters for ticket issuance");
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while issuing tickets");
            
            // Check if it's a 404 (order not found)
            if (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                return NotFound(new { error = "Flight order not found", details = ex.Message });
            }
            
            return StatusCode(StatusCodes.Status502BadGateway,
                new { error = "Failed to communicate with Amadeus API", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while issuing tickets");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred", details = ex.Message });
        }
    }

    /// <summary>
    /// Automated flight booking endpoint that creates a simple request, searches flights,
    /// selects the first result, prices it, and creates an order automatically
    /// </summary>
    /// <param name="request">Optional parameters for the automated booking. If not provided, uses defaults.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete booking flow result including search, pricing, and order</returns>
    [HttpPost("auto-book")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AutoBookFlight(
        [FromBody] AutoBookRequest? request = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Step 1: Create a simple request automatically with defaults if not provided
            var searchRequest = new FlightSearchRequest
            {
                TripType = request?.TripType ?? TripType.OneWay,
                OriginLocationCode = request?.OriginLocationCode ?? "NYC",
                DestinationLocationCode = request?.DestinationLocationCode ?? "LAX",
                DepartureDate = request?.DepartureDate ?? DateTime.Today.AddDays(30),
                ReturnDate = request?.ReturnDate,
                Adults = request?.Adults ?? 1,
                Children = request?.Children,
                Infants = request?.Infants,
                TravelClass = request?.TravelClass ?? "ECONOMY",
                NonStop = request?.NonStop ?? false,
                CurrencyCode = request?.CurrencyCode ?? "USD",
                MaxResults = request?.MaxResults ?? 10
            };

            _logger.LogInformation(
                "Auto-booking: Creating search request from {Origin} to {Destination} on {Date}",
                searchRequest.OriginLocationCode,
                searchRequest.DestinationLocationCode,
                searchRequest.DepartureDate);

            // Step 2: Call search flights
            var searchResult = await _flightSearchService.SearchFlightsAsync(searchRequest, cancellationToken);

            if (searchResult.Data == null || searchResult.Data.Count == 0)
            {
                return BadRequest(new { error = "No flights found for the given criteria" });
            }

            _logger.LogInformation(
                "Auto-booking: Found {Count} flight offers, selecting the first one",
                searchResult.Data.Count);

            // Step 3: Select the first response
            var selectedOffer = searchResult.Data[0];

            // Step 4: Call price
            var pricingResult = await _flightSearchService.PriceFlightOffersAsync(
                new List<FlightOffer> { selectedOffer },
                cancellationToken);

            if (pricingResult.Data?.FlightOffers == null || pricingResult.Data.FlightOffers.Count == 0)
            {
                return BadRequest(new { error = "Failed to price the selected flight offer" });
            }

            var pricedOffer = pricingResult.Data.FlightOffers[0];

            _logger.LogInformation(
                "Auto-booking: Successfully priced flight offer. Total price: {Price} {Currency}",
                pricedOffer.Price?.Total,
                pricedOffer.Price?.Currency);

            // Step 5: Call order with default traveler information
            var orderRequest = new FlightOrderRequest
            {
                Data = new FlightOrderData
                {
                    Type = "flight-order",
                    FlightOffers = new List<FlightOffer> { pricedOffer },
                    Travelers = CreateDefaultTravelers(searchRequest.Adults, request?.TravelerInfo),
                    Contacts = CreateDefaultContacts(request?.ContactInfo)
                }
            };

            var orderResult = await _flightOrderService.CreateFlightOrderAsync(orderRequest, cancellationToken);

            _logger.LogInformation(
                "Auto-booking: Successfully created order. Order ID: {OrderId}",
                orderResult.Data?.Id ?? "Unknown");

            return Ok(new
            {
                searchRequest = searchRequest,
                searchResult = new
                {
                    totalOffers = searchResult.Data.Count,
                    selectedOffer = new
                    {
                        id = selectedOffer.Id,
                        price = selectedOffer.Price
                    }
                },
                pricingResult = new
                {
                    totalPrice = pricedOffer.Price?.Total,
                    currency = pricedOffer.Price?.Currency
                },
                orderResult = orderResult
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters for auto-booking");
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred during auto-booking");
            return StatusCode(StatusCodes.Status502BadGateway,
                new { error = "Failed to communicate with Amadeus API", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during auto-booking");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An unexpected error occurred", details = ex.Message });
        }
    }

    private List<TravelerInfo> CreateDefaultTravelers(int adults, TravelerInfoRequest? customInfo = null)
    {
        var travelers = new List<TravelerInfo>();

        for (int i = 1; i <= adults; i++)
        {
            travelers.Add(new TravelerInfo
            {
                Id = i.ToString(),
                DateOfBirth = customInfo?.DateOfBirth ?? "1990-01-01",
                Name = new TravelerName
                {
                    FirstName = customInfo?.FirstName ?? "John",
                    LastName = customInfo?.LastName ?? "Doe"
                },
                Gender = customInfo?.Gender ?? "MALE",
                Contact = customInfo?.EmailAddress != null ? new TravelerContact
                {
                    EmailAddress = customInfo.EmailAddress,
                    Phones = customInfo.PhoneNumber != null ? new List<Phone>
                    {
                        new Phone
                        {
                            DeviceType = "MOBILE",
                            CountryCallingCode = customInfo.CountryCallingCode ?? "1",
                            Number = customInfo.PhoneNumber
                        }
                    } : null
                } : null
            });
        }

        return travelers;
    }

    private List<Contact>? CreateDefaultContacts(ContactInfoRequest? customInfo = null)
    {
        // Always create a contact with defaults if customInfo is null or email is not provided
        return new List<Contact>
        {
            new Contact
            {
                AddresseeName = new ContactName
                {
                    FirstName = customInfo?.FirstName ?? "John",
                    LastName = customInfo?.LastName ?? "Doe"
                },
                EmailAddress = customInfo?.EmailAddress ?? "test@example.com",
                Phones = customInfo?.PhoneNumber != null ? new List<Phone>
                {
                    new Phone
                    {
                        DeviceType = "MOBILE",
                        CountryCallingCode = customInfo.CountryCallingCode ?? "1",
                        Number = customInfo.PhoneNumber
                    }
                } : null,
                Purpose = "STANDARD"
            }
        };
    }
}

/// <summary>
/// Request model for automated flight booking
/// </summary>
public class AutoBookRequest
{
    public TripType? TripType { get; set; }
    public string? OriginLocationCode { get; set; }
    public string? DestinationLocationCode { get; set; }
    public DateTime? DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int? Adults { get; set; }
    public int? Children { get; set; }
    public int? Infants { get; set; }
    public string? TravelClass { get; set; }
    public bool? NonStop { get; set; }
    public string? CurrencyCode { get; set; }
    public int? MaxResults { get; set; }
    public TravelerInfoRequest? TravelerInfo { get; set; }
    public ContactInfoRequest? ContactInfo { get; set; }
}

/// <summary>
/// Traveler information for automated booking
/// </summary>
public class TravelerInfoRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CountryCallingCode { get; set; }
}

/// <summary>
/// Contact information for automated booking
/// </summary>
public class ContactInfoRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CountryCallingCode { get; set; }
}

