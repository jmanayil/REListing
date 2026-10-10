using REListing.Domain.Models;

namespace REListing.Services.Models
{
    // A search match together with its ranking (null when the search had no ranking criteria).
    public class RankedListing
    {
        public RankedListing(Listing listing, decimal? rank)
        {
            Listing = listing;
            Rank = rank;
        }

        public Listing Listing { get; }

        public decimal? Rank { get; }
    }
}
