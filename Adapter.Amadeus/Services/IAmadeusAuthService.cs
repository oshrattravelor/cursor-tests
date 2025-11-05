namespace Adapter.Amadeus.Services;

public interface IAmadeusAuthService
{
    Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}

