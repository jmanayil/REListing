using REListing.Domain.Models;
using REListing.Repositories.Contracts;
using REListing.Services.Contracts;
using REListing.Services.Models;

namespace REListing.Services
{
    // Business logic for listings; sits between the controller and the data source.
    public class ListingService : IListingService
    {
        private readonly IListingRepository _repository;
        private readonly IListingRankingService _rankingService;

        public ListingService(IListingRepository repository, IListingRankingService rankingService)
        {
            _repository = repository;
            _rankingService = rankingService;
        }

        public IReadOnlyList<Listing> GetAll() => _repository.GetAll();

        public Listing? GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return _repository.GetById(id.Trim());
        }

        public PagedResult<RankedListing> Search(ListingSearchCriteria criteria)
        {
            ArgumentNullException.ThrowIfNull(criteria);
            ArgumentOutOfRangeException.ThrowIfLessThan(criteria.Page, 1, nameof(criteria.Page));
            ArgumentOutOfRangeException.ThrowIfLessThan(criteria.PageSize, 1, nameof(criteria.PageSize));

            if (criteria.MinPrice > criteria.MaxPrice)
            {
                throw new ArgumentException(
                    $"MinPrice ({criteria.MinPrice}) cannot be greater than MaxPrice ({criteria.MaxPrice}).", nameof(criteria));
            }

            // City is the main filter, so the repository narrows the set before the remaining filters run here.
            var city = criteria.City?.Trim();
            IEnumerable<Listing> listings = string.IsNullOrEmpty(city)
                ? _repository.GetAll()
                : _repository.GetByCity(city);

            if (criteria.MinPrice is decimal minPrice)
            {
                listings = listings.Where(l => l.Price >= minPrice);
            }

            if (criteria.MaxPrice is decimal maxPrice)
            {
                listings = listings.Where(l => l.Price <= maxPrice);
            }

            if (criteria.MinBedrooms is int minBedrooms)
            {
                listings = listings.Where(l => l.Bedrooms >= minBedrooms);
            }

            var keyword = criteria.Keyword?.Trim();
            if (!string.IsNullOrEmpty(keyword))
            {
                listings = listings.Where(l =>
                    l.Description?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true);
            }

            // Highest rank first, then newest; Id breaks ties so paging is stable.
            // Without ranking criteria every rank is null, so the order is simply newest first.
            var matches = listings
                .Select(l => new RankedListing(
                    l, _rankingService.Rank(l, criteria.MinPrice, criteria.MaxPrice, criteria.MinBedrooms)))
                .OrderByDescending(r => r.Rank)
                .ThenByDescending(r => r.Listing.ListedDate)
                .ThenBy(r => r.Listing.Id, StringComparer.Ordinal)
                .ToList();

            // Computed as long so a very large page number can't overflow into a negative skip.
            var skip = (long)(criteria.Page - 1) * criteria.PageSize;
            var pageItems = matches
                .Skip((int)Math.Min(skip, int.MaxValue))
                .Take(criteria.PageSize)
                .ToList();

            return new PagedResult<RankedListing>(pageItems, criteria.Page, criteria.PageSize, matches.Count);
        }
    }
}
