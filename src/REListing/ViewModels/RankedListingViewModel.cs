namespace REListing.ViewModels
{
    // A listing in search results. Rank is null when the search had no ranking criteria
    // (a full price range and/or minBedrooms).
    public class RankedListingViewModel : ListingViewModel
    {
        public decimal? Rank { get; set; }
    }
}
