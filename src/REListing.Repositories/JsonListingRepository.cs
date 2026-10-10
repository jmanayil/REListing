using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using REListing.Domain.Models;
using REListing.Repositories.Contracts;
using REListing.Repositories.Options;

namespace REListing.Repositories
{
    // Loads listings from a JSON file once and serves them from memory.
    public class JsonListingRepository : IListingRepository
    {
        // Matches the source file format: camelCase property names and statuses as "active"/"pending".
        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) }
        };

        private readonly IReadOnlyList<Listing> _listings;

        public JsonListingRepository(IOptions<ListingDataOptions> options, ILogger<JsonListingRepository> logger)
        {
            var configuredPath = options.Value.FilePath;
            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                throw new InvalidOperationException($"'{ListingDataOptions.SectionName}:FilePath' is not configured.");
            }

            var fullPath = Path.Combine(AppContext.BaseDirectory, configuredPath);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Listing data file not found at '{fullPath}'.", fullPath);
            }

            try
            {
                using var stream = File.OpenRead(fullPath);
                _listings = JsonSerializer.Deserialize<List<Listing>>(stream, SerializerOptions) ?? new List<Listing>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Listing data file '{fullPath}' contains invalid JSON: {ex.Message}", ex);
            }

            logger.LogInformation("Loaded {Count} listings from {Path}", _listings.Count, fullPath);
        }

        public IReadOnlyList<Listing> GetAll() => _listings;

        public Listing? GetById(string id) =>
            _listings.FirstOrDefault(l => string.Equals(l.Id, id, StringComparison.OrdinalIgnoreCase));

        public IReadOnlyList<Listing> GetByCity(string city) =>
            _listings.Where(l => string.Equals(l.City, city, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
