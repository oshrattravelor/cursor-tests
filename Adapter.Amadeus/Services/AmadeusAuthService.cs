using System.Text.Json;
using Adapter.Amadeus.Configuration;
using Adapter.Amadeus.Models.Authentication;
using Microsoft.Extensions.Options;

namespace Adapter.Amadeus.Services;

public class AmadeusAuthService : IAmadeusAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AmadeusSettings _settings;
    private readonly IHttpRequestLogger _requestLogger;
    private AccessTokenResponse? _cachedToken;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public AmadeusAuthService(
        HttpClient httpClient, 
        IOptions<AmadeusSettings> settings,
        IHttpRequestLogger requestLogger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _requestLogger = requestLogger;

        var baseUrl = _settings.IsProduction
            ? "https://api.amadeus.com"
            : "https://test.travel.api.amadeus.com";

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
            var formData = new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _settings.ApiKey),
                new KeyValuePair<string, string>("client_secret", _settings.ApiSecret),
                new KeyValuePair<string, string>("Ama-Client-Ref", $"TRAVELOR BOOKING ENGINE-PDT-{DateTime.UtcNow.ToString()}")
            };
            
            var requestContent = new FormUrlEncodedContent(formData);
            var requestBody = string.Join("&", formData.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            var response = await _httpClient.PostAsync("/v1/security/oauth2/token", requestContent, cancellationToken);
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            // Log request and response
            var headers = new Dictionary<string, string>();
            if (response.RequestMessage?.Headers != null)
            {
                foreach (var header in response.RequestMessage.Headers)
                {
                    headers[header.Key] = string.Join(", ", header.Value);
                }
            }

            await _requestLogger.LogRequestResponseAsync(
                "Auth",
                "/v1/security/oauth2/token",
                "POST",
                requestBody,
                headers,
                responseContent,
                (int)response.StatusCode,
                isFormEncoded: true);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to obtain access token. Status: {response.StatusCode}, Error: {responseContent}");
            }
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

