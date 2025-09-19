using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace WellandPoolLeagueMud.Data.Services
{
    public class Auth0Settings
    {
        public const string SectionName = "Auth0";

        public string Domain { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public M2MSettings M2M { get; set; } = new();
    }

    public class M2MSettings
    {
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }

    public interface IAuth0TokenService
    {
        Task<string> GetManagementApiTokenAsync();
        Task<string> RefreshTokenAsync();
    }

    public class Auth0TokenService : IAuth0TokenService
    {
        private readonly Auth0Settings _auth0Settings;
        private readonly IMemoryCache _cache;
        private readonly ILogger<Auth0TokenService> _logger;
        private readonly AuthenticationApiClient _authClient;
        private readonly SemaphoreSlim _semaphore;

        private const string CacheKey = "auth0_management_token";
        private static readonly TimeSpan TokenCacheDuration = TimeSpan.FromMinutes(50); // Auth0 tokens typically last 1 hour

        public Auth0TokenService(
            IOptions<Auth0Settings> auth0Settings,
            IMemoryCache cache,
            ILogger<Auth0TokenService> logger)
        {
            _auth0Settings = auth0Settings.Value;
            _cache = cache;
            _logger = logger;
            _semaphore = new SemaphoreSlim(1, 1);

            if (string.IsNullOrEmpty(_auth0Settings.Domain))
                throw new ArgumentException("Auth0 Domain is required", nameof(auth0Settings));

            _authClient = new AuthenticationApiClient(_auth0Settings.Domain);
        }

        public async Task<string> GetManagementApiTokenAsync()
        {
            // Try to get token from cache first
            if (_cache.TryGetValue(CacheKey, out string? cachedToken) && !string.IsNullOrEmpty(cachedToken))
            {
                _logger.LogDebug("Using cached Auth0 management token");
                return cachedToken;
            }

            // Use semaphore to prevent multiple simultaneous token requests
            await _semaphore.WaitAsync();
            try
            {
                // Check cache again in case another thread already got the token
                if (_cache.TryGetValue(CacheKey, out cachedToken) && !string.IsNullOrEmpty(cachedToken))
                {
                    return cachedToken;
                }

                _logger.LogInformation("Fetching new Auth0 management token");
                return await RefreshTokenAsync();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<string> RefreshTokenAsync()
        {
            ValidateConfiguration();

            try
            {
                var tokenRequest = new ClientCredentialsTokenRequest
                {
                    ClientId = _auth0Settings.M2M.ClientId,
                    ClientSecret = _auth0Settings.M2M.ClientSecret,
                    Audience = $"https://{_auth0Settings.Domain}/api/v2/"
                };

                _logger.LogDebug("Requesting Auth0 management token for client: {ClientId}", _auth0Settings.M2M.ClientId);

                var tokenResponse = await _authClient.GetTokenAsync(tokenRequest);

                if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
                {
                    throw new InvalidOperationException("Failed to obtain access token from Auth0");
                }

                // Cache the token with expiration
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TokenCacheDuration,
                    Priority = CacheItemPriority.High
                };

                _cache.Set(CacheKey, tokenResponse.AccessToken, cacheOptions);

                _logger.LogInformation("Successfully obtained and cached Auth0 management token. Expires in: {Duration}", TokenCacheDuration);

                return tokenResponse.AccessToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to obtain Auth0 management token");
                throw new InvalidOperationException("Unable to authenticate with Auth0 Management API", ex);
            }
        }

        private void ValidateConfiguration()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(_auth0Settings.Domain))
                errors.Add("Auth0:Domain");

            if (string.IsNullOrEmpty(_auth0Settings.M2M.ClientId))
                errors.Add("Auth0:M2M:ClientId");

            if (string.IsNullOrEmpty(_auth0Settings.M2M.ClientSecret))
                errors.Add("Auth0:M2M:ClientSecret");

            if (errors.Any())
            {
                var missingSettings = string.Join(", ", errors);
                throw new InvalidOperationException($"Missing required Auth0 configuration settings: {missingSettings}");
            }
        }

        public void Dispose()
        {
            _semaphore?.Dispose();
            _authClient?.Dispose();
        }
    }

    // Factory for creating ManagementApiClient instances
    public interface IAuth0ManagementClientFactory
    {
        Task<Auth0.ManagementApi.IManagementApiClient> CreateClientAsync();
    }

    public class Auth0ManagementClientFactory : IAuth0ManagementClientFactory
    {
        private readonly IAuth0TokenService _tokenService;
        private readonly Auth0Settings _auth0Settings;
        private readonly ILogger<Auth0ManagementClientFactory> _logger;

        public Auth0ManagementClientFactory(
            IAuth0TokenService tokenService,
            IOptions<Auth0Settings> auth0Settings,
            ILogger<Auth0ManagementClientFactory> logger)
        {
            _tokenService = tokenService;
            _auth0Settings = auth0Settings.Value;
            _logger = logger;
        }

        public async Task<Auth0.ManagementApi.IManagementApiClient> CreateClientAsync()
        {
            try
            {
                var token = await _tokenService.GetManagementApiTokenAsync();
                return new Auth0.ManagementApi.ManagementApiClient(token, _auth0Settings.Domain);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Auth0 Management API client");
                throw;
            }
        }
    }
}