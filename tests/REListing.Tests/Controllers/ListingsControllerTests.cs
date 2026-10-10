using Microsoft.AspNetCore.Mvc;
using REListing.Controllers;
using REListing.Domain.Enums;
using REListing.Domain.Models;
using REListing.Services.Models;
using REListing.Tests.Fakes;
using REListing.ViewModels;

namespace REListing.Tests.Controllers
{
    public class ListingsControllerTests
    {
        private static readonly List<Listing> Listings = new()
        {
            new Listing { Id = "A1", City = "Springfield", Status = ListingStatus.Active },
            new Listing { Id = "B7", City = "Fairfax", Status = ListingStatus.Pending }
        };

        private readonly FakeListingService _service = new(Listings);
        private readonly ListingsController _controller;

        public ListingsControllerTests()
        {
            _controller = new ListingsController(_service);
        }

        [Fact]
        public void GetAll_ReturnsOkWithPagedViewModels()
        {
            var result = _controller.GetAll(new ListingSearchQuery());

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var page = Assert.IsType<PagedResponse<RankedListingViewModel>>(ok.Value);
            Assert.Equal(new[] { "A1", "B7" }, page.Items.Select(l => l.Id));
            Assert.Equal(1, page.Page);
            Assert.Equal(ListingSearchQuery.DefaultPageSize, page.PageSize);
            Assert.Equal(2, page.TotalCount);
            Assert.Equal(1, page.TotalPages);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(7.5)]
        public void GetAll_ReturnsRankFromService(double? rank)
        {
            _service.Rank = (decimal?)rank;

            var ok = Assert.IsType<OkObjectResult>(_controller.GetAll(new ListingSearchQuery()).Result);
            var page = Assert.IsType<PagedResponse<RankedListingViewModel>>(ok.Value);

            Assert.All(page.Items, l => Assert.Equal((decimal?)rank, l.Rank));
        }

        [Fact]
        public void GetAll_NoQueryValues_SearchesWithNoFiltersAndDefaultPaging()
        {
            _controller.GetAll(new ListingSearchQuery());

            var criteria = Assert.IsType<ListingSearchCriteria>(_service.LastSearchCriteria);
            Assert.Null(criteria.City);
            Assert.Null(criteria.MinPrice);
            Assert.Null(criteria.MaxPrice);
            Assert.Null(criteria.MinBedrooms);
            Assert.Null(criteria.Keyword);
            Assert.Equal(1, criteria.Page);
            Assert.Equal(ListingSearchQuery.DefaultPageSize, criteria.PageSize);
        }

        [Fact]
        public void GetAll_PassesPagingToService()
        {
            _controller.GetAll(new ListingSearchQuery { Page = 3, PageSize = 5 });

            var criteria = Assert.IsType<ListingSearchCriteria>(_service.LastSearchCriteria);
            Assert.Equal(3, criteria.Page);
            Assert.Equal(5, criteria.PageSize);
        }

        [Fact]
        public void GetAll_PassesAllFiltersToService()
        {
            _controller.GetAll(new ListingSearchQuery
            {
                City = "Reston",
                MinPrice = 500000m,
                MaxPrice = 600000m,
                MinBedrooms = 3,
                Keyword = "pool"
            });

            var criteria = Assert.IsType<ListingSearchCriteria>(_service.LastSearchCriteria);
            Assert.Equal("Reston", criteria.City);
            Assert.Equal(500000m, criteria.MinPrice);
            Assert.Equal(600000m, criteria.MaxPrice);
            Assert.Equal(3, criteria.MinBedrooms);
            Assert.Equal("pool", criteria.Keyword);
        }

        [Fact]
        public void GetAll_NoListings_ReturnsOkWithEmptyList()
        {
            var controller = new ListingsController(new FakeListingService(new List<Listing>()));

            var ok = Assert.IsType<OkObjectResult>(controller.GetAll(new ListingSearchQuery()).Result);
            var page = Assert.IsType<PagedResponse<RankedListingViewModel>>(ok.Value);
            Assert.Empty(page.Items);
            Assert.Equal(0, page.TotalCount);
            Assert.Equal(0, page.TotalPages);
        }

        [Fact]
        public void GetById_ExistingId_ReturnsOkWithViewModel()
        {
            var result = _controller.GetById("B7");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var listing = Assert.IsType<ListingViewModel>(ok.Value);
            Assert.Equal("B7", listing.Id);
            Assert.Equal(ListingStatus.Pending, listing.Status);
        }

        [Fact]
        public void GetById_UnknownId_ReturnsNotFound()
        {
            var result = _controller.GetById("ZZZ");

            Assert.IsType<NotFoundResult>(result.Result);
        }
    }
}
