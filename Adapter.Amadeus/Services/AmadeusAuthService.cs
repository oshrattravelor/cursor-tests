using System.Text.Json;
using Adapter.Amadeus.Configuration;
using Adapter.Amadeus.Models.Authentication;
using Microsoft.Extensions.Options;

namespace Adapter.Amadeus.Services;

public class AmadeusAuthService : IAmadeusAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AmadeusSettings _settings;
    private AccessTokenResponse? _cachedToken;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public AmadeusAuthService(HttpClient httpClient, IOptions<AmadeusSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        
        var baseUrl = _settings.IsProduction 
            ? "https://api.amadeus.com" 
            : "https://test.api.amadeus.com";
        
        _httpClient.BaseAddress = new Uri(baseUrl);
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        
        try
        {
            // Check if we have a valid cached token
            if (_cachedToken != null && DateTime.UtcNow < _cachedToken.ExpiresAt.AddMinutes(-5))
            {
                return _cachedToken.AccessToken;
            }

            // Request a new token
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _settings.ApiKey),
                new KeyValuePair<string, string>("client_secret", _settings.ApiSecret)
            });

            var response = await _httpClient.PostAsync("/v1/security/oauth2/token", requestContent, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new HttpRequestException($"Failed to obtain access token. Status: {response.StatusCode}, Error: {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var tokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(responseContent);

            if (tokenResponse == null)
            {
                throw new InvalidOperationException("Failed to deserialize token response");
            }

            tokenResponse.ExpiresAt = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
            _cachedToken = tokenResponse;

            return _cachedToken.AccessToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}

