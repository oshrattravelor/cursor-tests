# Quick Start Guide

Get up and running with Amadeus flight search in 5 minutes!

## 1. Get API Keys (2 minutes)

1. Visit [https://developers.amadeus.com](https://developers.amadeus.com)
2. Register/Login → "My Self-Service Workspace" → "Create new app"
3. Copy your **API Key** and **API Secret**

## 2. Configure (1 minute)

**Option A - User Secrets (Recommended)**:
```bash
cd BeAgentTravelApi
dotnet user-secrets set "Amadeus:ApiKey" "your-key"
dotnet user-secrets set "Amadeus:ApiSecret" "your-secret"
```

**Option B - Edit appsettings.Development.json**:
```json
{
  "Amadeus": {
    "ApiKey": "paste-your-key-here",
    "ApiSecret": "paste-your-secret-here",
    "BaseUrl": "https://test.api.amadeus.com",
    "IsProduction": false
  }
}
```

## 3. Run (1 minute)

```bash
cd BeAgentTravelApi
dotnet run
```

## 4. Test (1 minute)

Open browser: `https://localhost:7000/swagger`

Or use curl:
```bash
curl -X POST https://localhost:7000/api/flights/search \
  -H "Content-Type: application/json" \
  -d '{
    "originLocationCode": "NYC",
    "destinationLocationCode": "LAX", 
    "departureDate": "2024-12-15",
    "adults": 1,
    "maxResults": 5
  }'
```

## That's it! 🎉

You should see flight results from Amadeus.

### Common Issues

❌ **"Unauthorized"** → Check your API credentials  
❌ **"No flights found"** → Try NYC to LAX, dates in future  
❌ **SSL errors** → Run `dotnet dev-certs https --trust`

### What You Can Search

- **One-way or Round-trip** flights
- **Multiple passengers** (adults, children, infants)
- **Travel classes**: ECONOMY, BUSINESS, FIRST
- **Non-stop** flights only (optional)
- **Different currencies**: USD, EUR, GBP, etc.

### Example Searches

**Round-trip Business Class**:
```json
{
  "originLocationCode": "JFK",
  "destinationLocationCode": "LHR",
  "departureDate": "2024-12-15",
  "returnDate": "2024-12-22",
  "adults": 2,
  "travelClass": "BUSINESS"
}
```

**Non-stop Only**:
```json
{
  "originLocationCode": "LAX",
  "destinationLocationCode": "SFO",
  "departureDate": "2024-12-01",
  "adults": 1,
  "nonStop": true
}
```

### Next Steps

- Read [SETUP.md](SETUP.md) for detailed setup instructions
- Read [README.md](README.md) for full documentation
- Check [Adapter.Amadeus/README.md](Adapter.Amadeus/README.md) for adapter details
- Use [flights.http](BeAgentTravelApi/flights.http) for more examples

### Need Help?

- 📖 [Amadeus API Docs](https://developers.amadeus.com/self-service)
- 💬 [Amadeus Forum](https://developers.amadeus.com/forum)
- 🐛 Check [SETUP.md#troubleshooting](SETUP.md#troubleshooting)

