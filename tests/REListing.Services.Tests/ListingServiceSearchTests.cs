using REListing.Domain.Models;
using REListing.Services.Models;
using REListing.Services.Tests.Fakes;

namespace REListing.Services.Tests
{
    public class ListingServiceSearchTests
    {
        // Listed newest-first, so results come back in this order after the service's date sort.
        private static readonly List<Listing> Listings = new()
        {
            new Listing { Id = "S1", City = "Springfield", Price = 450000m, Bedrooms = 2, ListedDate = new DateOnly(2026, 9, 5), Description = "Top-floor condo. Pet friendly." },
            new Listing { Id = "S2", City = "Springfield", Price = 525000m, Bedrooms = 3, ListedDate = new DateOnly(2026, 9, 4), Description = "Updated kitchen, fenced yard." },
            new Listing { Id = "F1", City = "Fairfax", Price = 399000m, Bedrooms = 2, ListedDate = new DateOnly(2026, 9, 3), Description = "Cozy starter home, no pets." },
            new Listing { Id = "R1", City = "Reston", Price = 610000m, Bedrooms = 4, ListedDate = new DateOnly(2026, 9, 2), Description = "Spacious family home. Pets welcome." },
            new Listing { Id = "R2", City = "Reston", Price = 585000m, Bedrooms = 3, ListedDate = new DateOnly(2026, 9, 1), Description = null }
        };

        private readonly FakeListingRepository _repository = new(Listings);
        private readonly ListingService _service;

        public ListingServiceSearchTests()
        {
            _service = new ListingService(_repository, new FakeListingRankingService());
        }

        private IEnumerable<string> SearchIds(ListingSearchCriteria criteria) => _service.Search(criteria).Items.Select(r => r.Listing.Id);

        private static ListingService ServiceFor(List<Listing> listings, FakeListingRankingService? ranking = null) =>
            new(new FakeListingRepository(listings), ranking ?? new FakeListingRankingService());

        [Fact]
        public void Search_NoFilters_ReturnsAllListingsFromGetAll()
        {
            Assert.Equal(new[] { "S1", "S2", "F1", "R1", "R2" }, SearchIds(new ListingSearchCriteria()));
            Assert.Equal(1, _repository.GetAllCallCount);
            Assert.Empty(_repository.RequestedCities);
        }

        [Fact]
        public void Search_WithCity_PassesTrimmedCityToRepositoryInsteadOfGetAll()
        {
            var ids = SearchIds(new ListingSearchCriteria { City = "  reston " });

            Assert.Equal(new[] { "R1", "R2" }, ids);
            Assert.Equal(new[] { "reston" }, _repository.RequestedCities);
            Assert.Equal(0, _repository.GetAllCallCount);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Search_BlankCity_UsesGetAll(string city)
        {
            _service.Search(new ListingSearchCriteria { City = city });

            Assert.Equal(1, _repository.GetAllCallCount);
            Assert.Empty(_repository.RequestedCities);
        }

        [Fact]
        public void Search_UnknownCity_ReturnsEmpty()
        {
            Assert.Empty(SearchIds(new ListingSearchCriteria { City = "Atlantis" }));
        }

        [Fact]
        public void Search_MinPrice_IsInclusive()
        {
            Assert.Equal(new[] { "S2", "R1", "R2" }, SearchIds(new ListingSearchCriteria { MinPrice = 525000m }));
        }

        [Fact]
        public void Search_MaxPrice_IsInclusive()
        {
            Assert.Equal(new[] { "S1", "F1" }, SearchIds(new ListingSearchCriteria { MaxPrice = 450000m }));
        }

        [Fact]
        public void Search_PriceRange_ReturnsListingsWithinRange()
        {
            Assert.Equal(new[] { "S2", "R2" }, SearchIds(new ListingSearchCriteria { MinPrice = 500000m, MaxPrice = 600000m }));
        }

        [Fact]
        public void Search_MinPriceEqualsMaxPrice_ReturnsExactMatches()
        {
            Assert.Equal(new[] { "F1" }, SearchIds(new ListingSearchCriteria { MinPrice = 399000m, MaxPrice = 399000m }));
        }

        [Fact]
        public void Search_MinPriceGreaterThanMaxPrice_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                _service.Search(new ListingSearchCriteria { MinPrice = 600000m, MaxPrice = 500000m }));
        }

        [Fact]
        public void Search_NullCriteria_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _service.Search(null!));
        }

        [Fact]
        public void Search_MinBedrooms_IsInclusive()
        {
            Assert.Equal(new[] { "S2", "R1", "R2" }, SearchIds(new ListingSearchCriteria { MinBedrooms = 3 }));
        }

        [Theory]
        [InlineData("pet")]
        [InlineData("PET")]
        [InlineData("  pet  ")]
        public void Search_Keyword_MatchesDescriptionIgnoringCaseAndWhitespace(string keyword)
        {
            // "pet" also appears inside "pets", so the no-pets listing matches too.
            Assert.Equal(new[] { "S1", "F1", "R1" }, SearchIds(new ListingSearchCriteria { Keyword = keyword }));
        }

        [Fact]
        public void Search_Keyword_SkipsListingsWithoutDescription()
        {
            Assert.DoesNotContain("R2", SearchIds(new ListingSearchCriteria { Keyword = "home" }));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Search_BlankKeyword_IsIgnored(string keyword)
        {
            Assert.Equal(5, _service.Search(new ListingSearchCriteria { Keyword = keyword }).Items.Count);
        }

        [Fact]
        public void Search_AllFilters_AppliesThemTogether()
        {
            var criteria = new ListingSearchCriteria
            {
                City = "Reston",
                MinPrice = 500000m,
                MaxPrice = 700000m,
                MinBedrooms = 4,
                Keyword = "family"
            };

            Assert.Equal(new[] { "R1" }, SearchIds(criteria));
        }

        [Fact]
        public void Search_DefaultPaging_ReturnsFirstPageOfTwenty()
        {
            var result = _service.Search(new ListingSearchCriteria());

            Assert.Equal(1, result.Page);
            Assert.Equal(20, result.PageSize);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(1, result.TotalPages);
            Assert.Equal(5, result.Items.Count);
        }

        [Theory]
        [InlineData(1, new[] { "S1", "S2" })]
        [InlineData(2, new[] { "F1", "R1" })]
        [InlineData(3, new[] { "R2" })]
        public void Search_PageSizeTwo_ReturnsRequestedPage(int page, string[] expectedIds)
        {
            var result = _service.Search(new ListingSearchCriteria { Page = page, PageSize = 2 });

            Assert.Equal(expectedIds, result.Items.Select(r => r.Listing.Id));
            Assert.Equal(page, result.Page);
            Assert.Equal(2, result.PageSize);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
        }

        [Fact]
        public void Search_PageBeyondLast_ReturnsEmptyItemsWithTotals()
        {
            var result = _service.Search(new ListingSearchCriteria { Page = 4, PageSize = 2 });

            Assert.Empty(result.Items);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
        }

        [Fact]
        public void Search_VeryLargePage_ReturnsEmptyInsteadOfOverflowing()
        {
            var result = _service.Search(new ListingSearchCriteria { Page = int.MaxValue, PageSize = 100 });

            Assert.Empty(result.Items);
            Assert.Equal(5, result.TotalCount);
        }

        [Fact]
        public void Search_PagesAfterFiltering_TotalCountsOnlyMatches()
        {
            var result = _service.Search(new ListingSearchCriteria { City = "Reston", Page = 2, PageSize = 1 });

            Assert.Equal(new[] { "R2" }, result.Items.Select(r => r.Listing.Id));
            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.TotalPages);
        }

        [Fact]
        public void Search_NoMatches_ReturnsZeroTotalPages()
        {
            var result = _service.Search(new ListingSearchCriteria { City = "Atlantis" });

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.TotalPages);
        }

        [Fact]
        public void Search_OrdersByListedDateNewestFirst()
        {
            var service = ServiceFor(new List<Listing>
            {
                new() { Id = "OLD", ListedDate = new DateOnly(2026, 7, 1) },
                new() { Id = "NEW", ListedDate = new DateOnly(2026, 9, 1) },
                new() { Id = "MID", ListedDate = new DateOnly(2026, 8, 1) }
            });

            var ids = service.Search(new ListingSearchCriteria()).Items.Select(r => r.Listing.Id);

            Assert.Equal(new[] { "NEW", "MID", "OLD" }, ids);
        }

        [Fact]
        public void Search_SameListedDate_OrdersById()
        {
            var sameDay = new DateOnly(2026, 9, 1);
            var service = ServiceFor(new List<Listing>
            {
                new() { Id = "C", ListedDate = sameDay },
                new() { Id = "A", ListedDate = sameDay },
                new() { Id = "B", ListedDate = sameDay }
            });

            var ids = service.Search(new ListingSearchCriteria()).Items.Select(r => r.Listing.Id);

            Assert.Equal(new[] { "A", "B", "C" }, ids);
        }

        [Fact]
        public void Search_SortsBeforePaging()
        {
            var service = ServiceFor(new List<Listing>
            {
                new() { Id = "OLD", ListedDate = new DateOnly(2026, 7, 1) },
                new() { Id = "NEW", ListedDate = new DateOnly(2026, 9, 1) },
                new() { Id = "MID", ListedDate = new DateOnly(2026, 8, 1) }
            });

            var firstPage = service.Search(new ListingSearchCriteria { Page = 1, PageSize = 1 });
            var lastPage = service.Search(new ListingSearchCriteria { Page = 3, PageSize = 1 });

            Assert.Equal("NEW", Assert.Single(firstPage.Items).Listing.Id);
            Assert.Equal("OLD", Assert.Single(lastPage.Items).Listing.Id);
        }

        // --- Ranking ---

        [Fact]
        public void Search_PassesRankingCriteriaToRankingServiceForEachMatch()
        {
            var ranking = new FakeListingRankingService();
            var service = new ListingService(_repository, ranking);

            service.Search(new ListingSearchCriteria { City = "Reston", MinPrice = 500000m, MaxPrice = 700000m, MinBedrooms = 3 });

            Assert.Equal(new[]
            {
                ("R1", (decimal?)500000m, (decimal?)700000m, (int?)3),
                ("R2", (decimal?)500000m, (decimal?)700000m, (int?)3)
            }, ranking.Calls.OrderBy(c => c.Id));
        }

        [Fact]
        public void Search_ReturnsRankWithEachListing()
        {
            var ranking = new FakeListingRankingService(new() { ["S1"] = 7.5m, ["S2"] = 2m });
            var service = new ListingService(_repository, ranking);

            var result = service.Search(new ListingSearchCriteria { City = "Springfield" });

            Assert.Equal(new decimal?[] { 7.5m, 2m }, result.Items.Select(r => r.Rank));
        }

        [Fact]
        public void Search_NoRankingCriteria_RanksAreNullAndOrderIsNewestFirst()
        {
            var result = _service.Search(new ListingSearchCriteria());

            Assert.All(result.Items, r => Assert.Null(r.Rank));
            Assert.Equal(new[] { "S1", "S2", "F1", "R1", "R2" }, result.Items.Select(r => r.Listing.Id));
        }

        [Fact]
        public void Search_OrdersByRankHighestFirst()
        {
            // Test data is newest-first S1..R2; ranks reverse that.
            var ranking = new FakeListingRankingService(new()
            {
                ["S1"] = 1m, ["S2"] = 2m, ["F1"] = 3m, ["R1"] = 4m, ["R2"] = 5m
            });
            var service = new ListingService(_repository, ranking);

            var ids = service.Search(new ListingSearchCriteria()).Items.Select(r => r.Listing.Id);

            Assert.Equal(new[] { "R2", "R1", "F1", "S2", "S1" }, ids);
        }

        [Fact]
        public void Search_EqualRanks_OrderNewestFirst()
        {
            var ranking = new FakeListingRankingService(new()
            {
                ["S1"] = 5m, ["S2"] = 9m, ["F1"] = 5m, ["R1"] = 9m, ["R2"] = 5m
            });
            var service = new ListingService(_repository, ranking);

            var ids = service.Search(new ListingSearchCriteria()).Items.Select(r => r.Listing.Id);

            // 9s first (S2 newer than R1), then 5s newest first (S1, F1, R2).
            Assert.Equal(new[] { "S2", "R1", "S1", "F1", "R2" }, ids);
        }

        [Fact]
        public void Search_RanksBeforePaging()
        {
            var ranking = new FakeListingRankingService(new() { ["R2"] = 10m, ["S1"] = 0m });
            var service = new ListingService(_repository, ranking);

            var firstPage = service.Search(new ListingSearchCriteria { Page = 1, PageSize = 1 });

            // R2 is the oldest listing but has the highest rank, so it leads page 1.
            Assert.Equal("R2", Assert.Single(firstPage.Items).Listing.Id);
        }

        [Fact]
        public void Search_WithRealRankingService_OrdersByCombinedPriceAndBedroomRank()
        {
            var ranking = new ListingRankingService(
                Microsoft.Extensions.Options.Options.Create(new Options.RankingOptions()));
            var service = new ListingService(_repository, ranking);

            // Range 500k–700k, min 3 bedrooms. Matches: S2 525k/3bd, R1 610k/4bd, R2 585k/3bd.
            // S2: price 8.75 + 0 = 8.75. R1: price 4.5 + 3 = 7.5. R2: price 5.75 + 0 = 5.75.
            var result = service.Search(new ListingSearchCriteria { MinPrice = 500000m, MaxPrice = 700000m, MinBedrooms = 3 });

            Assert.Equal(new[] { "S2", "R1", "R2" }, result.Items.Select(r => r.Listing.Id));
            Assert.Equal(new decimal?[] { 8.75m, 7.5m, 5.75m }, result.Items.Select(r => r.Rank));
        }

        [Theory]
        [InlineData(0, 20)]
        [InlineData(-1, 20)]
        [InlineData(1, 0)]
        [InlineData(1, -5)]
        public void Search_InvalidPaging_Throws(int page, int pageSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _service.Search(new ListingSearchCriteria { Page = page, PageSize = pageSize }));
        }
    }
}
