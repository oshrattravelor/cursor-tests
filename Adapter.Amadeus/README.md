# Amadeus GDS Adapter

This adapter provides integration with Amadeus GDS (Global Distribution System) for searching flights using their REST API.

## Features

- OAuth 2.0 authentication with token caching
- Flight search functionality
- Comprehensive flight offer models
- Easy integration with ASP.NET Core applications

## Getting Started

### 1. Get Amadeus API Credentials

1. Sign up for a free developer account at [Amadeus for Developers](https://developers.amadeus.com)
2. Create a new app to get your API Key and API Secret
3. Start with the test environment before moving to production

### 2. Configure Your Application

Add your Amadeus credentials to `appsettings.json`:

```json
{
  "Amadeus": {
    "ApiKey": "YOUR_AMADEUS_API_KEY",
    "ApiSecret": "YOUR_AMADEUS_API_SECRET",
    "BaseUrl": "https://test.api.amadeus.com",
    "IsProduction": false
  }
}
```

### 3. Register Services

In your `Program.cs`:

```csharp
using Adapter.Amadeus.Extensions;

builder.Services.AddAmadeusServices(settings =>
{
    builder.Configuration.GetSection("Amadeus").Bind(settings);
});
```

### 4. Use in Your Controllers

```csharp
using Adapter.Amadeus.Services;
using Adapter.Amadeus.Models.FlightSearch;

public class FlightsController : ControllerBase
{
    private readonly IAmadeusFlightSearchService _flightSearchService;

    public FlightsController(IAmadeusFlightSearchService flightSearchService)
    {
        _flightSearchService = flightSearchService;
    }

    [HttpPost("search")]
    public async Task<IActionResult> SearchFlights([FromBody] FlightSearchRequest request)
    {
        var result = await _flightSearchService.SearchFlightsAsync(request);
        return Ok(result);
    }
}
```

## Flight Search Parameters

The `FlightSearchRequest` model supports the following parameters:

- `OriginLocationCode` (required): IATA code of origin (e.g., "NYC", "JFK")
- `DestinationLocationCode` (required): IATA code of destination (e.g., "LAX")
- `DepartureDate` (required): Date of departure
- `ReturnDate` (optional): Date of return (for round-trip)
- `Adults` (required): Number of adult travelers (default: 1)
- `Children` (optional): Number of children (2-11 years)
- `Infants` (optional): Number of infants (under 2 years)
- `TravelClass` (optional): ECONOMY, PREMIUM_ECONOMY, BUSINESS, FIRST
- `NonStop` (optional): true for non-stop flights only (default: false)
- `CurrencyCode` (optional): Currency for pricing (default: "USD")
- `MaxResults` (optional): Maximum number of results (default: 10, max: 250)

## Example Request

```json
{
  "originLocationCode": "NYC",
  "destinationLocationCode": "LAX",
  "departureDate": "2024-12-15",
  "returnDate": "2024-12-22",
  "adults": 1,
  "travelClass": "ECONOMY",
  "nonStop": false,
  "currencyCode": "USD",
  "maxResults": 10
}
```

## Important Notes

### Test vs Production

- Test Environment: `https://test.api.amadeus.com`
  - Free tier available
  - Limited data but real API behavior
  - Good for development and testing

- Production Environment: `https://api.amadeus.com`
  - Requires paid subscription
  - Full access to live flight data
  - Set `IsProduction: true` in configuration

### Rate Limits

Test environment rate limits (as of 2024):
- 10 transactions per second (TPS)
- Limited monthly quota based on your plan

### IATA Codes

Use 3-letter IATA codes for airports or cities:
- City codes (NYC, PAR, LON) - searches all airports in that city
- Airport codes (JFK, LAX, LHR) - searches specific airport

### Date Format

All dates should be in `yyyy-MM-dd` format (e.g., "2024-12-15").

## Authentication

The adapter handles OAuth 2.0 authentication automatically:
- Tokens are cached and reused until expiration
- Automatic token refresh
- Thread-safe token management

## Error Handling

The adapter throws `HttpRequestException` for API communication errors. Implement proper error handling in your controllers:

```csharp
try
{
    var result = await _flightSearchService.SearchFlightsAsync(request);
    return Ok(result);
}
catch (HttpRequestException ex)
{
    return StatusCode(502, new { error = "Failed to communicate with Amadeus API" });
}
```

## Additional Resources

- [Amadeus API Documentation](https://developers.amadeus.com/self-service)
- [Flight Offers Search API](https://developers.amadeus.com/self-service/category/flights/api-doc/flight-offers-search)
- [IATA Code Search](https://www.iata.org/en/publications/directories/code-search/)

