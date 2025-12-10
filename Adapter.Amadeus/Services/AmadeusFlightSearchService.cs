using System.Text;
using System.Text.Json;
using System.Web;
using Adapter.Amadeus.Configuration;
using Adapter.Amadeus.Models.FlightSearch;
using Microsoft.Extensions.Options;

namespace Adapter.Amadeus.Services;

public class AmadeusFlightSearchService : IAmadeusFlightSearchService
{
    private readonly HttpClient _httpClient;
    private readonly IAmadeusAuthService _authService;
    private readonly AmadeusSettings _settings;
    private readonly JsonSerializerOptions _jsonOptions;

    public AmadeusFlightSearchService(
        HttpClient httpClient, 
        IAmadeusAuthService authService,
        IOptions<AmadeusSettings> settings)
    {
        _httpClient = httpClient;
        _authService = authService;
        _settings = settings.Value;
        
        var baseUrl = _settings.IsProduction 
            ? "https://api.amadeus.com" 
            : "https://test.travel.api.amadeus.com";
        
        _httpClient.BaseAddress = new Uri(baseUrl);
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<FlightSearchResponse> SearchFlightsAsync(
        FlightSearchRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Get access token
        var accessToken = await _authService.GetAccessTokenAsync(cancellationToken);

        // Determine trip type
        var tripType = request.GetTripType();
        
        HttpRequestMessage httpRequest;
        
        if (tripType == TripType.MultiCity)
        {
            // Multi-city requires POST with JSON body
            httpRequest = BuildMultiCityRequest(request, accessToken);
        }
        else
        {
            // One-way and round-trip use GET with query parameters
            httpRequest = BuildSimpleRequest(request, tripType, accessToken);
        }

        // Send request
        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Flight search failed. Status: {response.StatusCode}, Error: {errorContent}");
        }

        // Parse response
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var flightSearchResponse = JsonSerializer.Deserialize<FlightSearchResponse>(
            responseContent, 
            new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

        if (flightSearchResponse == null)
        {
            throw new InvalidOperationException("Failed to deserialize flight search response");
        }

        return flightSearchResponse;
    }

    private HttpRequestMessage BuildSimpleRequest(
        FlightSearchRequest request, 
        TripType tripType, 
        string accessToken)
    {
        var queryParams = BuildQueryParameters(request, tripType);
        var requestUri = $"/v2/shopping/flight-offers?{queryParams}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
        httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

        return httpRequest;
    }

    private HttpRequestMessage BuildMultiCityRequest(
        FlightSearchRequest request, 
        string accessToken)
    {
        if (request.Segments == null || request.Segments.Count == 0)
        {
            throw new ArgumentException("Segments are required for multi-city flights");
        }

        // Build the request body for multi-city
        var requestBody = new
        {
            originDestinations = request.Segments.Select(s => new
            {
                id = (request.Segments.IndexOf(s) + 1).ToString(),
                originLocationCode = s.OriginLocationCode,
                destinationLocationCode = s.DestinationLocationCode,
                departureDateTimeRange = new
                {
                    date = s.DepartureDate.ToString("yyyy-MM-dd")
                }
            }).ToArray(),
            travelers = BuildTravelers(request),
            sources = new[] { "GDS" },
            searchCriteria = new
            {
                maxFlightOffers = request.MaxResults,
                flightFilters = new
                {
                    cabinRestrictions = request.TravelClass != null ? new[]
                    {
                        new { cabin = request.TravelClass }
                    } : null,
                    carrierRestrictions = (object?)null,
                    connectionRestriction = request.NonStop ? new
                    {
                        maxNumberOfConnections = 0
                    } : null
                }
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v2/shopping/flight-offers")
        {
            Content = content
        };
        httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

        return httpRequest;
    }

    private object[] BuildTravelers(FlightSearchRequest request)
    {
        var travelers = new List<object>();
        
        // Add adults
        for (int i = 0; i < request.Adults; i++)
        {
            travelers.Add(new
            {
                id = (travelers.Count + 1).ToString(),
                travelerType = "ADULT"
            });
        }
        
        // Add children
        if (request.Children.HasValue)
        {
            for (int i = 0; i < request.Children.Value; i++)
            {
                travelers.Add(new
                {
                    id = (travelers.Count + 1).ToString(),
                    travelerType = "CHILD"
                });
            }
        }
        
        // Add infants
        if (request.Infants.HasValue)
        {
            for (int i = 0; i < request.Infants.Value; i++)
            {
                travelers.Add(new
                {
                    id = (travelers.Count + 1).ToString(),
                    travelerType = "HELD_INFANT",
                    associatedAdultId = (i + 1).ToString() // Infants must be associated with an adult
                });
            }
        }
        
        return travelers.ToArray();
    }

    private string BuildQueryParameters(FlightSearchRequest request, TripType tripType)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        
        if (tripType == TripType.MultiCity)
        {
            throw new InvalidOperationException("Multi-city flights should use POST request, not GET");
        }
        
        if (string.IsNullOrEmpty(request.OriginLocationCode))
        {
            throw new ArgumentException("OriginLocationCode is required for one-way and round-trip flights");
        }
        
        if (string.IsNullOrEmpty(request.DestinationLocationCode))
        {
            throw new ArgumentException("DestinationLocationCode is required for one-way and round-trip flights");
        }
        
        if (!request.DepartureDate.HasValue)
        {
            throw new ArgumentException("DepartureDate is required for one-way and round-trip flights");
        }
        
        queryParams["originLocationCode"] = request.OriginLocationCode;
        queryParams["destinationLocationCode"] = request.DestinationLocationCode;
        queryParams["departureDate"] = request.DepartureDate.Value.ToString("yyyy-MM-dd");
        
        if (tripType == TripType.RoundTrip && request.ReturnDate.HasValue)
        {
            queryParams["returnDate"] = request.ReturnDate.Value.ToString("yyyy-MM-dd");
        }
        
        queryParams["adults"] = request.Adults.ToString();
        
        if (request.Children.HasValue && request.Children.Value > 0)
        {
            queryParams["children"] = request.Children.Value.ToString();
        }
        
        if (request.Infants.HasValue && request.Infants.Value > 0)
        {
            queryParams["infants"] = request.Infants.Value.ToString();
        }
        
        if (!string.IsNullOrEmpty(request.TravelClass))
        {
            queryParams["travelClass"] = request.TravelClass;
        }
        
        queryParams["nonStop"] = request.NonStop.ToString().ToLower();
        queryParams["currencyCode"] = request.CurrencyCode;
        queryParams["max"] = request.MaxResults.ToString();

        return queryParams.ToString() ?? string.Empty;
    }

    public async Task<FlightOffersPricingResponse> PriceFlightOffersAsync(
        List<FlightOffer> flightOffers, 
        CancellationToken cancellationToken = default)
    {
        if (flightOffers == null || flightOffers.Count == 0)
        {
            throw new ArgumentException("At least one flight offer is required for pricing", nameof(flightOffers));
        }

        // Get access token
        var accessToken = await _authService.GetAccessTokenAsync(cancellationToken);

        // Build the request body
        var requestBody = new FlightOffersPricingRequest
        {
            Data = new FlightOffersPricingData
            {
                Type = "flight-offers-pricing",
                FlightOffers = flightOffers
            }
        };

        var jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Add query parameter to include detailed fare rules
        var requestUri = "/v1/shopping/flight-offers/pricing?include=detailed-fare-rules";
        
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = content
        };
        httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

        // Send request
        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Flight offers pricing failed. Status: {response.StatusCode}, Error: {errorContent}");
        }

        // Parse response
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var pricingResponse = JsonSerializer.Deserialize<FlightOffersPricingResponse>(
            responseContent, 
            new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });

        if (pricingResponse == null)
        {
            throw new InvalidOperationException("Failed to deserialize flight offers pricing response");
        }

        return pricingResponse;
    }
}

