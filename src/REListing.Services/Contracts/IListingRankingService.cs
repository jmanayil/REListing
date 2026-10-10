using REListing.Domain.Models;

namespace REListing.Services.Contracts
{
    public interface IListingRankingService
    {
        // Scores how well a listing fits the requested price range and bedrooms, from 0 (worst) to 10 (best).
        // Returns null when there is nothing to rank by.
        decimal? Rank(Listing listing, decimal? minPrice, decimal? maxPrice, int? minBedrooms);
    }
}
