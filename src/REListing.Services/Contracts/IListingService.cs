using REListing.Domain.Models;
using REListing.Services.Models;

namespace REListing.Services.Contracts
{
    public interface IListingService
    {
        IReadOnlyList<Listing> GetAll();

        Listing? GetById(string id);

        // Filters, ranks and pages listings. Results are ordered by rank (highest first), then newest first.
        PagedResult<RankedListing> Search(ListingSearchCriteria criteria);
    }
}
