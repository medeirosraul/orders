using Microsoft.Extensions.Logging;

namespace Orders.Core.Security
{
    public interface IAuthorizer
    {
        Task<bool> Authorize(string key);
    }

    public class Authorizer : IAuthorizer
    {
        private readonly ILogger<Authorizer> _logger;

        public Authorizer(ILogger<Authorizer> logger)
        {
            _logger = logger;
        }

        public Task<bool> Authorize(string key)
        {
            _logger.LogInformation("Authorizing key: {Key}", key);

            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Authorization failed: Key is null or empty.");
                return Task.FromResult(false);
            }

            bool isAuthorized = key == "valid-key";

            if (isAuthorized)
            {
                _logger.LogInformation("Authorization successful for key: {Key}", key);
            }
            else
            {
                _logger.LogWarning("Authorization failed for key: {Key}", key);
            }

            return Task.FromResult(isAuthorized);
        }
    }
}