using REListing.Domain.Models;
using REListing.Services.Models;
using REListing.ViewModels;

namespace REListing.Mappings
{
    public static class ListingMappings
    {
        public static ListingSearchCriteria ToCriteria(this ListingSearchQuery query) => new()
        {
            City = query.City,
            MinPrice = query.MinPrice,
            MaxPrice = query.MaxPrice,
            MinBedrooms = query.MinBedrooms,
            Keyword = query.Keyword,
            Page = query.Page,
            PageSize = query.PageSize
        };

        public static PagedResponse<RankedListingViewModel> ToViewModel(this PagedResult<RankedListing> result) => new()
        {
            Items = result.Items.Select(r => r.ToViewModel()).ToList(),
            Page = result.Page,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
            TotalPages = result.TotalPages
        };

        public static RankedListingViewModel ToViewModel(this RankedListing ranked)
        {
            var viewModel = ranked.Listing.MapTo<RankedListingViewModel>();
            viewModel.Rank = ranked.Rank;
            return viewModel;
        }

        public static ListingViewModel ToViewModel(this Listing listing) => listing.MapTo<ListingViewModel>();

        // Copies the listing fields into any ListingViewModel type, so derived view models don't repeat them.
        private static T MapTo<T>(this Listing listing) where T : ListingViewModel, new() => new()
        {
            Id = listing.Id,
            Source = listing.Source,
            Address = listing.Address,
            City = listing.City,
            State = listing.State,
            Zip = listing.Zip,
            Price = listing.Price,
            Bedrooms = listing.Bedrooms,
            Bathrooms = listing.Bathrooms,
            Sqft = listing.Sqft,
            Latitude = listing.Latitude,
            Longitude = listing.Longitude,
            ListedDate = listing.ListedDate,
            Status = listing.Status,
            Description = listing.Description
        };
    }
}
