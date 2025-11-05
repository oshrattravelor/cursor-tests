# Setup Guide

This guide will help you set up the BeAgent Travel API with Amadeus GDS integration.

## Step 1: Get Amadeus API Credentials

1. Go to [Amadeus for Developers](https://developers.amadeus.com)
2. Click "Register" to create a free account
3. Once logged in, go to "My Self-Service Workspace"
4. Click "Create new app"
5. Fill in the app details:
   - **Application Name**: BeAgent Travel API (or your preferred name)
   - **Application Type**: Flight Search
6. Click "Create" and you'll see your credentials:
   - **API Key** (also called Client ID)
   - **API Secret** (also called Client Secret)
7. Copy these credentials - you'll need them in the next step

## Step 2: Configure API Credentials

You have three options for configuring your credentials:

### Option 1: User Secrets (Recommended for Development)

This is the most secure option for local development as it keeps secrets outside your project directory.

```bash
cd BeAgentTravelApi
dotnet user-secrets init
dotnet user-secrets set "Amadeus:ApiKey" "your-api-key-here"
dotnet user-secrets set "Amadeus:ApiSecret" "your-api-secret-here"
```

The settings in appsettings.json will serve as defaults/placeholders.

### Option 2: appsettings.Development.json (Local Development)

**⚠️ Important**: Make sure this file is in `.gitignore` if you commit to source control!

Edit `BeAgentTravelApi/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Amadeus": {
    "ApiKey": "paste-your-actual-api-key-here",
    "ApiSecret": "paste-your-actual-api-secret-here",
    "BaseUrl": "https://test.api.amadeus.com",
    "IsProduction": false
  }
}
```

### Option 3: Environment Variables (Production)

For production deployments (Azure, AWS, Docker, etc.), use environment variables:

**Windows (PowerShell)**:
```powershell
$env:Amadeus__ApiKey = "your-api-key"
$env:Amadeus__ApiSecret = "your-api-secret"
$env:Amadeus__IsProduction = "false"
```

**Linux/Mac**:
```bash
export Amadeus__ApiKey="your-api-key"
export Amadeus__ApiSecret="your-api-secret"
export Amadeus__IsProduction="false"
```

**Docker**:
```dockerfile
ENV Amadeus__ApiKey="your-api-key"
ENV Amadeus__ApiSecret="your-api-secret"
ENV Amadeus__IsProduction="false"
```

**Azure App Service**:
Add these as Application Settings in the Azure Portal.

## Step 3: Install Dependencies

```bash
# From the solution root
dotnet restore
```

## Step 4: Build the Solution

```bash
dotnet build
```

If you see any build errors, make sure you have .NET 8 SDK installed:
```bash
dotnet --version
# Should show 8.0.x or higher
```

## Step 5: Run the API

```bash
cd BeAgentTravelApi
dotnet run
```

You should see output like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7000
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

## Step 6: Test the API

### Using Swagger UI

1. Open your browser and go to: `https://localhost:7000/swagger`
2. You'll see the interactive API documentation
3. Try the following endpoints:

   **Get Sample Request**:
   - Click on `GET /api/flights/sample-request`
   - Click "Try it out"
   - Click "Execute"
   - You should get a sample flight search request

   **Search Flights**:
   - Click on `POST /api/flights/search`
   - Click "Try it out"
   - Modify the request body or use the default
   - Click "Execute"
   - You should see flight offers from Amadeus

### Using the HTTP File

If you're using Visual Studio Code with REST Client extension or Visual Studio 2022+:

1. Open `BeAgentTravelApi/flights.http`
2. Update the `@baseUrl` variable if needed (default is `https://localhost:7000`)
3. Click "Send Request" on any of the examples
4. View the response in the output pane

### Using curl

```bash
# Get sample request
curl https://localhost:7000/api/flights/sample-request

# Search flights
curl -X POST https://localhost:7000/api/flights/search \
  -H "Content-Type: application/json" \
  -d '{
    "originLocationCode": "NYC",
    "destinationLocationCode": "LAX",
    "departureDate": "2024-12-15",
    "adults": 1,
    "travelClass": "ECONOMY",
    "nonStop": false,
    "currencyCode": "USD",
    "maxResults": 5
  }'
```

## Step 7: Verify It Works

A successful response should look like:

```json
{
  "meta": {
    "count": 5
  },
  "data": [
    {
      "id": "1",
      "source": "GDS",
      "price": {
        "currency": "USD",
        "total": "123.45",
        ...
      },
      "itineraries": [
        {
          "segments": [
            {
              "departure": {
                "iataCode": "JFK",
                "at": "2024-12-15T08:00:00"
              },
              ...
            }
          ]
        }
      ]
    }
  ]
}
```

## Troubleshooting

### "Unauthorized" or "Invalid credentials"

**Problem**: Your API credentials are incorrect or not being read properly.

**Solutions**:
1. Verify your credentials in Amadeus portal
2. Make sure there are no extra spaces or quotes in your configuration
3. If using User Secrets, verify they're set correctly:
   ```bash
   dotnet user-secrets list
   ```
4. Restart the API after changing configuration

### "SSL Certificate" errors

**Problem**: HTTPS certificate issues on localhost.

**Solution**:
```bash
dotnet dev-certs https --trust
```

### "No flight offers found"

**Problem**: No flights match your search criteria.

**Solutions**:
1. Check that airport codes are valid (use IATA codes like "JFK", "LAX")
2. Make sure dates are in the future
3. Try removing the `NonStop` filter
4. Try different routes (popular routes like NYC-LAX usually have more results)
5. Increase `MaxResults` to see more options

### "Rate limit exceeded"

**Problem**: You've exceeded the API rate limit.

**Solutions**:
1. Wait a few minutes before trying again
2. Check your Amadeus account quota
3. Reduce the frequency of requests during testing
4. Consider implementing caching for repeated searches

### "Connection refused" or "Unable to connect"

**Problem**: The Amadeus API is unreachable.

**Solutions**:
1. Check your internet connection
2. Verify the `BaseUrl` in your configuration:
   - Test: `https://test.api.amadeus.com`
   - Production: `https://api.amadeus.com`
3. Check if Amadeus API is down: [Amadeus Status](https://developers.amadeus.com)

### Build errors

**Problem**: The project won't build.

**Solutions**:
1. Ensure .NET 8 SDK is installed: `dotnet --version`
2. Clean and rebuild:
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```
3. Delete `bin` and `obj` folders and rebuild

## Testing Tips

### Valid Airport/City Codes

Use these for testing:

**US Airports**:
- JFK (John F. Kennedy, New York)
- LAX (Los Angeles)
- ORD (O'Hare, Chicago)
- MIA (Miami)
- SFO (San Francisco)

**City Codes** (search all airports in city):
- NYC (New York - JFK, LGA, EWR)
- LON (London - LHR, LGW, etc.)
- PAR (Paris - CDG, ORY)

**International**:
- LHR (Heathrow, London)
- CDG (Charles de Gaulle, Paris)
- DXB (Dubai)
- SIN (Singapore)
- NRT (Narita, Tokyo)

### Suggested Test Searches

1. **Short domestic flight**:
   - LAX to SFO
   - JFK to BOS

2. **Cross-country**:
   - NYC to LAX
   - MIA to SEA

3. **International**:
   - JFK to LHR
   - LAX to NRT

4. **Popular routes** (usually have many results):
   - NYC to LAX
   - LAX to LAS
   - ORD to LAX

## Next Steps

Once everything is working:

1. **Explore the response data**: Look at the detailed flight information
2. **Try different search parameters**: Different classes, number of passengers, etc.
3. **Implement caching**: Cache popular routes to reduce API calls
4. **Add error handling**: Improve error messages for your users
5. **Build a frontend**: Create a web interface for the flight search
6. **Add more features**: 
   - Flight details endpoint
   - Price alerts
   - Multi-city searches
   - Fare rules and baggage information

## Moving to Production

Before going to production:

1. **Get Production API Access**: Contact Amadeus sales for production credentials
2. **Update Configuration**:
   ```json
   {
     "Amadeus": {
       "ApiKey": "production-api-key",
       "ApiSecret": "production-api-secret",
       "BaseUrl": "https://api.amadeus.com",
       "IsProduction": true
     }
   }
   ```
3. **Use Environment Variables**: Never hardcode production credentials
4. **Implement Rate Limiting**: Protect your API quota
5. **Add Caching**: Reduce costs and improve performance
6. **Add Authentication**: Secure your API endpoints
7. **Monitor Usage**: Track your API usage and costs
8. **Error Handling**: Implement comprehensive error handling and logging

## Support

- **Amadeus API Issues**: [Amadeus Support](https://developers.amadeus.com/support)
- **Project Issues**: Create an issue in the repository
- **Documentation**: Check the [README.md](README.md) for more details

## Resources

- [Amadeus API Documentation](https://developers.amadeus.com/self-service)
- [Flight Offers Search API](https://developers.amadeus.com/self-service/category/flights/api-doc/flight-offers-search)
- [IATA Airport Codes](https://www.iata.org/en/publications/directories/code-search/)
- [ASP.NET Core Configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

