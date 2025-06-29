using Microsoft.Extensions.Configuration;

namespace Orders.Core.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string Require(this IConfiguration configuration, string key)
        {
            var value = configuration[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Configuration key '{key}' is required but not set.");
            }

            return value;
        }
    }
}