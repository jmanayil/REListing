using Microsoft.Extensions.Options;
using REListing.Domain.Models;
using REListing.Services.Contracts;
using REListing.Services.Options;

namespace REListing.Services
{
    public class ListingRankingService : IListingRankingService
    {
        public const decimal MaxPriceScore = 10m;

        private readonly decimal _bedroomWeight;

        public ListingRankingService(IOptions<RankingOptions> options)
        {
            _bedroomWeight = options.Value.BedroomWeight;
        }

        public decimal? Rank(Listing listing, decimal? minPrice, decimal? maxPrice, int? minBedrooms)
        {
            ArgumentNullException.ThrowIfNull(listing);

            if (minPrice > maxPrice)
            {
                throw new ArgumentException($"minPrice ({minPrice}) cannot be greater than maxPrice ({maxPrice}).", nameof(minPrice));
            }

            // Each part is scored only when its criteria were given; the ranking is their sum.
            var priceRanking = minPrice is decimal min && maxPrice is decimal max
                ? GetPriceRanking(listing.Price, min, max)
                : (decimal?)null;

            var bedroomRanking = minBedrooms is int requestedBedrooms
                ? GetBedroomRanking(listing.Bedrooms, requestedBedrooms)
                : (decimal?)null;

            if (priceRanking is null && bedroomRanking is null)
            {
                return null;
            }

            return Math.Round((priceRanking ?? 0m) + (bedroomRanking ?? 0m), 2);
        }

        // 10 at minPrice, falling linearly to 0 at maxPrice. Prices outside the range are clamped.
        private static decimal GetPriceRanking(decimal price, decimal minPrice, decimal maxPrice)
        {
            if (price <= minPrice)
            {
                return MaxPriceScore;
            }

            if (price >= maxPrice)
            {
                return 0m;
            }

            return MaxPriceScore * (maxPrice - price) / (maxPrice - minPrice);
        }

        // BedroomWeight points for each bedroom above the requested minimum (negative if below it).
        private decimal GetBedroomRanking(int bedrooms, int minBedrooms)
        {
            return (bedrooms - minBedrooms) * _bedroomWeight;
        }
    }
}
