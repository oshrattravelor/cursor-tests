namespace Adapter.Amadeus.Services;

/// <summary>
/// Service for logging HTTP requests and responses to the file system
/// </summary>
public interface IHttpRequestLogger
{
    /// <summary>
    /// Logs an HTTP request and response to the file system
    /// </summary>
    /// <param name="serviceName">Name of the service making the request (e.g., "Auth", "FlightSearch")</param>
    /// <param name="endpoint">The API endpoint (e.g., "/v1/security/oauth2/token")</param>
    /// <param name="method">HTTP method (GET, POST, etc.)</param>
    /// <param name="requestBody">Request body as string (null for GET requests or form-encoded)</param>
    /// <param name="requestHeaders">Request headers as dictionary</param>
    /// <param name="responseBody">Response body as string</param>
    /// <param name="statusCode">HTTP status code</param>
    /// <param name="isFormEncoded">Whether the request is form-encoded (for auth requests)</param>
    Task LogRequestResponseAsync(
        string serviceName,
        string endpoint,
        string method,
        string? requestBody,
        Dictionary<string, string>? requestHeaders,
        string responseBody,
        int statusCode,
        bool isFormEncoded = false);
}

