using System.Text;
using System.Text.Json;
using Adapter.Amadeus.Configuration;
using Microsoft.Extensions.Options;

namespace Adapter.Amadeus.Services;

public class HttpRequestLogger : IHttpRequestLogger
{
    private readonly AmadeusSettings _settings;
    private readonly string _logsDirectory;
    private readonly JsonSerializerOptions _jsonOptions;

    public HttpRequestLogger(IOptions<AmadeusSettings> settings)
    {
        _settings = settings.Value;
        
        // Create logs directory - try to use current directory first, fallback to base directory
        var baseDirectory = Directory.GetCurrentDirectory();
        if (string.IsNullOrEmpty(baseDirectory) || !Directory.Exists(baseDirectory))
        {
            baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }
        
        _logsDirectory = Path.Combine(baseDirectory, "logs", "amadeus-requests");
        
        // Ensure directory exists
        Directory.CreateDirectory(_logsDirectory);
        
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
    }

    public async Task LogRequestResponseAsync(
        string serviceName,
        string endpoint,
        string method,
        string? requestBody,
        Dictionary<string, string>? requestHeaders,
        string responseBody,
        int statusCode,
        bool isFormEncoded = false)
    {
        try
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            var sanitizedEndpoint = SanitizeFileName(endpoint);
            var fileName = $"{timestamp}_{serviceName}_{sanitizedEndpoint}.json";
            var filePath = Path.Combine(_logsDirectory, fileName);

            var logEntry = new
            {
                timestamp = DateTime.UtcNow.ToString("O"),
                service = serviceName,
                endpoint = endpoint,
                method = method,
                request = new
                {
                    headers = requestHeaders,
                    body = isFormEncoded ? requestBody : TryParseJson(requestBody)
                },
                response = new
                {
                    statusCode = statusCode,
                    body = TryParseJson(responseBody)
                }
            };

            var jsonContent = JsonSerializer.Serialize(logEntry, _jsonOptions);
            await File.WriteAllTextAsync(filePath, jsonContent, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            // Silently fail logging to avoid breaking the main functionality
            // In production, you might want to log this to a proper logging system
            Console.WriteLine($"Failed to log HTTP request/response: {ex.Message}");
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
        return sanitized.Length > 50 ? sanitized.Substring(0, 50) : sanitized;
    }

    private static object? TryParseJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<object>(json);
        }
        catch
        {
            // If it's not valid JSON, return as string
            return json;
        }
    }
}

