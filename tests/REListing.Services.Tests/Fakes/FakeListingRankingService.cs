using REListing.Domain.Models;
using REListing.Services.Contracts;

namespace REListing.Services.Tests.Fakes
{
    // Returns preset ranks by listing Id (null for any Id not set), so tests control the rank order directly.
    public class FakeListingRankingService : IListingRankingService
    {
        private readonly Dictionary<string, decimal?> _ranksById;

        public FakeListingRankingService(Dictionary<string, decimal?>? ranksById = null)
        {
            _ranksById = ranksById ?? new Dictionary<string, decimal?>();
        }

        // Arguments of each Rank call, so tests can check what the search passed along.
        public List<(string Id, decimal? MinPrice, decimal? MaxPrice, int? MinBedrooms)> Calls { get; } = new();

        public decimal? Rank(Listing listing, decimal? minPrice, decimal? maxPrice, int? minBedrooms)
        {
            Calls.Add((listing.Id, minPrice, maxPrice, minBedrooms));
            return _ranksById.TryGetValue(listing.Id, out var rank) ? rank : null;
        }
    }
}
