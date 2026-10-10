using REListing.Domain.Enums;
using REListing.Domain.Models;
using REListing.Mappings;
using REListing.Services.Models;

namespace REListing.Tests.Mappings
{
    public class ListingMappingsTests
    {
        [Fact]
        public void ToViewModel_CopiesAllFields()
        {
            var listing = new Listing
            {
                Id = "A1",
                Source = "MLS_A",
                Address = "123 Main St, Apt 4B",
                City = "Springfield",
                State = "VA",
                Zip = "22150",
                Price = 450000m,
                Bedrooms = 2,
                Bathrooms = 1.5m,
                Sqft = 980,
                Latitude = 38.7893,
                Longitude = -77.1873,
                ListedDate = new DateOnly(2026, 8, 29),
                Status = ListingStatus.Pending,
                Description = "Bright top-floor condo."
            };

            var viewModel = listing.ToViewModel();

            Assert.Equal(listing.Id, viewModel.Id);
            Assert.Equal(listing.Source, viewModel.Source);
            Assert.Equal(listing.Address, viewModel.Address);
            Assert.Equal(listing.City, viewModel.City);
            Assert.Equal(listing.State, viewModel.State);
            Assert.Equal(listing.Zip, viewModel.Zip);
            Assert.Equal(listing.Price, viewModel.Price);
            Assert.Equal(listing.Bedrooms, viewModel.Bedrooms);
            Assert.Equal(listing.Bathrooms, viewModel.Bathrooms);
            Assert.Equal(listing.Sqft, viewModel.Sqft);
            Assert.Equal(listing.Latitude, viewModel.Latitude);
            Assert.Equal(listing.Longitude, viewModel.Longitude);
            Assert.Equal(listing.ListedDate, viewModel.ListedDate);
            Assert.Equal(listing.Status, viewModel.Status);
            Assert.Equal(listing.Description, viewModel.Description);
        }

        [Fact]
        public void ToViewModel_PagedResult_CopiesItemsRanksAndPagingTotals()
        {
            var items = new List<RankedListing>
            {
                new(new Listing { Id = "F1" }, 8.5m),
                new(new Listing { Id = "R1" }, null)
            };
            var result = new PagedResult<RankedListing>(items, page: 2, pageSize: 2, totalCount: 5);

            var response = result.ToViewModel();

            Assert.Equal(new[] { "F1", "R1" }, response.Items.Select(l => l.Id));
            Assert.Equal(new decimal?[] { 8.5m, null }, response.Items.Select(l => l.Rank));
            Assert.Equal(2, response.Page);
            Assert.Equal(2, response.PageSize);
            Assert.Equal(5, response.TotalCount);
            Assert.Equal(3, response.TotalPages);
        }

        [Fact]
        public void ToViewModel_RankedListing_CopiesAllListingFieldsAndRank()
        {
            var listing = new Listing
            {
                Id = "A1", Source = "MLS_A", Address = "123 Main St", City = "Springfield", State = "VA", Zip = "22150",
                Price = 450000m, Bedrooms = 2, Bathrooms = 1.5m, Sqft = 980, Latitude = 38.7893, Longitude = -77.1873,
                ListedDate = new DateOnly(2026, 8, 29), Status = ListingStatus.Active, Description = "Condo."
            };

            var viewModel = new RankedListing(listing, 6.25m).ToViewModel();

            Assert.Equal(6.25m, viewModel.Rank);
            Assert.Equivalent(listing.ToViewModel(), viewModel);
        }
    }
}
