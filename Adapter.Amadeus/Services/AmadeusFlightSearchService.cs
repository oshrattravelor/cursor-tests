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
            : "https://test.api.amadeus.com";
        
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<FlightSearchResponse> SearchFlightsAsync(
        FlightSearchRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Get access token
        var accessToken = await _authService.GetAccessTokenAsync(cancellationToken);

        // Build query parameters
        var queryParams = BuildQueryParameters(request);
        var requestUri = $"/v2/shopping/flight-offers?{queryParams}";

        // Create HTTP request
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
        httpRequest.Headers.Add("Authorization", $"Bearer {accessToken}");

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

    private string BuildQueryParameters(FlightSearchRequest request)
    {
        var queryParams = HttpUtility.ParseQueryString(string.Empty);
        
        queryParams["originLocationCode"] = request.OriginLocationCode;
        queryParams["destinationLocationCode"] = request.DestinationLocationCode;
        queryParams["departureDate"] = request.DepartureDate.ToString("yyyy-MM-dd");
        
        if (request.ReturnDate.HasValue)
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
}

