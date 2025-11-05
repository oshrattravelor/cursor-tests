# BeAgent Travel API with Amadeus GDS Integration

A .NET 8 Web API that integrates with Amadeus GDS (Global Distribution System) for searching flights. This solution provides a clean architecture with an adapter pattern for easy integration with the Amadeus REST API.

## 🏗️ Solution Structure

```
cursor-tests/
├── Adapter.Amadeus/              # Amadeus GDS adapter library
│   ├── Configuration/            # Configuration classes
│   ├── Models/                   # Request/Response DTOs
│   │   ├── Authentication/       # OAuth 2.0 models
│   │   └── FlightSearch/         # Flight search models
│   ├── Services/                 # Service implementations
│   └── Extensions/               # Dependency injection extensions
└── BeAgentTravelApi/             # ASP.NET Core Web API
    └── Controllers/              # API controllers
```

## ✨ Features

- **Flight Search**: Search for flights using Amadeus GDS
- **OAuth 2.0 Authentication**: Automatic token management with caching
- **Comprehensive Models**: Detailed flight offer information including:
  - Pricing and fees
  - Itineraries and segments
  - Carrier information
  - Baggage allowance
  - Cabin class details
- **Flexible Search**: Support for:
  - One-way and round-trip flights
  - Multiple passengers (adults, children, infants)
  - Different travel classes
  - Non-stop flight filtering
  - Currency selection
- **Clean Architecture**: Separation of concerns with adapter pattern
- **Swagger UI**: Interactive API documentation

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK
- Amadeus API credentials (get them from [Amadeus for Developers](https://developers.amadeus.com))

### 1. Clone and Setup

```bash
git clone <your-repository>
cd cursor-tests
```

### 2. Configure Amadeus Credentials

Update `BeAgentTravelApi/appsettings.json` or `appsettings.Development.json`:

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

**Important**: Never commit your actual API credentials to source control!

### 3. Restore and Build

```bash
dotnet restore
dotnet build
```

### 4. Run the API

```bash
cd BeAgentTravelApi
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7000` (default)
- HTTP: `http://localhost:5000` (default)
- Swagger UI: `https://localhost:7000/swagger`

## 📖 Usage

### Using Swagger UI

1. Navigate to `https://localhost:7000/swagger`
2. Try the `/api/flights/sample-request` endpoint to get a sample request
3. Use the `/api/flights/search` endpoint to search for flights

### Using HTTP File

The solution includes a `flights.http` file with example requests. You can use it with:
- Visual Studio 2022+ (built-in support)
- VS Code with REST Client extension
- JetBrains Rider

### Example API Request

```http
POST https://localhost:7000/api/flights/search
Content-Type: application/json

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

### Example Response Structure

```json
{
  "meta": {
    "count": 10
  },
  "data": [
    {
      "id": "1",
      "source": "GDS",
      "oneWay": false,
      "price": {
        "currency": "USD",
        "total": "350.50",
        "base": "300.00",
        "grandTotal": "350.50"
      },
      "itineraries": [
        {
          "duration": "PT5H30M",
          "segments": [
            {
              "departure": {
                "iataCode": "JFK",
                "at": "2024-12-15T08:00:00"
              },
              "arrival": {
                "iataCode": "LAX",
                "at": "2024-12-15T11:30:00"
              },
              "carrierCode": "AA",
              "number": "123"
            }
          ]
        }
      ]
    }
  ]
}
```

## 🔑 Getting Amadeus API Credentials

1. Visit [Amadeus for Developers](https://developers.amadeus.com)
2. Create a free account
3. Go to "My Self-Service Workspace"
4. Click "Create new app"
5. Copy your API Key and API Secret
6. Use the **Test** environment for development (free tier)

### Test vs Production

| Environment | URL | Features |
|------------|-----|----------|
| Test | `https://test.api.amadeus.com` | Free tier, limited data, good for development |
| Production | `https://api.amadeus.com` | Paid subscription, live data, production use |

## 📋 API Endpoints

### Flights Controller

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/flights/search` | Search for flights |
| GET | `/api/flights/sample-request` | Get a sample request for testing |

## 🔧 Configuration Options

### AmadeusSettings

| Property | Type | Description | Default |
|----------|------|-------------|---------|
| ApiKey | string | Your Amadeus API key | - |
| ApiSecret | string | Your Amadeus API secret | - |
| BaseUrl | string | API base URL | `https://test.api.amadeus.com` |
| IsProduction | bool | Use production environment | `false` |

### FlightSearchRequest Parameters

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| OriginLocationCode | string | ✓ | IATA code (e.g., "NYC", "JFK") |
| DestinationLocationCode | string | ✓ | IATA code (e.g., "LAX") |
| DepartureDate | DateTime | ✓ | Departure date |
| ReturnDate | DateTime | | Return date (for round-trip) |
| Adults | int | ✓ | Number of adults (default: 1) |
| Children | int | | Number of children (2-11 years) |
| Infants | int | | Number of infants (under 2 years) |
| TravelClass | string | | ECONOMY, PREMIUM_ECONOMY, BUSINESS, FIRST |
| NonStop | bool | | Filter for non-stop flights |
| CurrencyCode | string | | Currency (default: "USD") |
| MaxResults | int | | Max results (default: 10, max: 250) |

## 🏛️ Architecture

The solution follows a clean architecture pattern:

1. **Adapter.Amadeus**: Pure business logic for Amadeus integration
   - No ASP.NET Core dependencies
   - Can be reused in other projects
   - Fully testable

2. **BeAgentTravelApi**: Web API layer
   - Controllers for HTTP endpoints
   - Configuration and dependency injection
   - API-specific concerns

### Dependency Injection

The adapter is registered using an extension method:

```csharp
builder.Services.AddAmadeusServices(settings =>
{
    builder.Configuration.GetSection("Amadeus").Bind(settings);
});
```

This registers:
- `IAmadeusAuthService` → Authentication service
- `IAmadeusFlightSearchService` → Flight search service
- Configured HttpClient instances with proper lifecycle management

## 🛠️ Development

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Publishing

```bash
dotnet publish -c Release
```

## 📚 Additional Resources

- [Amadeus API Documentation](https://developers.amadeus.com/self-service)
- [Flight Offers Search API](https://developers.amadeus.com/self-service/category/flights/api-doc/flight-offers-search)
- [IATA Airport Codes](https://www.iata.org/en/publications/directories/code-search/)

## 🔐 Security Notes

1. **Never commit API credentials** to source control
2. Use **environment variables** or **Azure Key Vault** for production
3. Enable **HTTPS** in production
4. Implement **rate limiting** to avoid API quota issues
5. Add **authentication/authorization** before deploying

## 📝 Environment Variables (Production)

Instead of appsettings.json, use environment variables:

```bash
export Amadeus__ApiKey="your-api-key"
export Amadeus__ApiSecret="your-api-secret"
export Amadeus__IsProduction="true"
```

Or in Azure App Service, add these as Application Settings.

## 🤝 Contributing

Feel free to submit issues and enhancement requests!

## 📄 License

[Your License Here]

## 🎯 Roadmap

Future enhancements:
- [ ] Flight booking functionality
- [ ] Hotel search integration
- [ ] Car rental integration
- [ ] Flight price prediction
- [ ] Email notifications
- [ ] Caching layer for frequently searched routes
- [ ] Rate limiting middleware
- [ ] Comprehensive unit tests
- [ ] Integration tests
- [ ] Docker support

## 💡 Tips

1. **IATA Codes**: Use city codes (NYC) to search all airports in a city, or specific airport codes (JFK) for a single airport
2. **Date Format**: Always use ISO format (yyyy-MM-dd) for dates
3. **Rate Limits**: The test environment has rate limits - implement caching for production
4. **Error Handling**: The API returns detailed error messages for troubleshooting
5. **Currency**: Results can be returned in different currencies - useful for international applications

## 🐛 Troubleshooting

### "Unauthorized" Error
- Check your API credentials in appsettings.json
- Ensure your Amadeus app is active
- Verify you're using the correct environment (test vs production)

### "No flight offers found"
- Check IATA codes are valid
- Ensure dates are in the future
- Try increasing MaxResults
- Remove NonStop filter if set

### "Rate limit exceeded"
- Wait a moment before retrying
- Implement caching in your application
- Consider upgrading your Amadeus plan

## 📞 Support

For Amadeus API issues:
- [Amadeus Support](https://developers.amadeus.com/support)
- [Amadeus Community Forums](https://developers.amadeus.com/forum)

For project issues:
- Create an issue in this repository

