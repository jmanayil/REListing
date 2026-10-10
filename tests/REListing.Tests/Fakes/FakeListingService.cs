using REListing.Domain.Models;
using REListing.Services.Contracts;
using REListing.Services.Models;

namespace REListing.Tests.Fakes
{
    public class FakeListingService : IListingService
    {
        private readonly IReadOnlyList<Listing> _listings;

        public FakeListingService(IReadOnlyList<Listing> listings) => _listings = listings;

        public IReadOnlyList<Listing> GetAll() => _listings;

        public Listing? GetById(string id) => _listings.FirstOrDefault(l => l.Id == id);

        // Criteria passed to the last Search call, so tests can check what the controller sent.
        public ListingSearchCriteria? LastSearchCriteria { get; private set; }

        // Rank returned with every listing from Search (null = no ranking criteria).
        public decimal? Rank { get; set; }

        // Returns every listing as a single page; filtering, ranking and paging are tested in the service tests.
        public PagedResult<RankedListing> Search(ListingSearchCriteria criteria)
        {
            LastSearchCriteria = criteria;
            var items = _listings.Select(l => new RankedListing(l, Rank)).ToList();
            return new PagedResult<RankedListing>(items, criteria.Page, criteria.PageSize, items.Count);
        }
    }
}
