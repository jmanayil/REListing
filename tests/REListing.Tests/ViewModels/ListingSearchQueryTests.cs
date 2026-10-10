using System.ComponentModel.DataAnnotations;
using REListing.ViewModels;

namespace REListing.Tests.ViewModels
{
    // Runs the same data-annotation validation [ApiController] uses to return 400 Bad Request.
    public class ListingSearchQueryTests
    {
        private static List<ValidationResult> Validate(ListingSearchQuery query)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(query, new ValidationContext(query), results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void Validate_NoFilters_IsValid()
        {
            Assert.Empty(Validate(new ListingSearchQuery()));
        }

        [Theory]
        [InlineData(450000, 525000)]
        [InlineData(450000, 450000)]
        [InlineData(0, 0)]
        public void Validate_ValidPriceRange_IsValid(int minPrice, int maxPrice)
        {
            Assert.Empty(Validate(new ListingSearchQuery { MinPrice = minPrice, MaxPrice = maxPrice }));
        }

        [Fact]
        public void Validate_OnlyMinPrice_IsValid()
        {
            Assert.Empty(Validate(new ListingSearchQuery { MinPrice = 500000m }));
        }

        [Fact]
        public void Validate_MinPriceGreaterThanMaxPrice_IsInvalid()
        {
            var results = Validate(new ListingSearchQuery { MinPrice = 600000m, MaxPrice = 500000m });

            var error = Assert.Single(results);
            Assert.Contains(nameof(ListingSearchQuery.MinPrice), error.MemberNames);
        }

        [Fact]
        public void Validate_NegativeMinPrice_IsInvalid()
        {
            var error = Assert.Single(Validate(new ListingSearchQuery { MinPrice = -1m }));
            Assert.Contains(nameof(ListingSearchQuery.MinPrice), error.MemberNames);
        }

        [Fact]
        public void Validate_NegativeMaxPrice_IsInvalid()
        {
            var error = Assert.Single(Validate(new ListingSearchQuery { MaxPrice = -1m }));
            Assert.Contains(nameof(ListingSearchQuery.MaxPrice), error.MemberNames);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        public void Validate_NonNegativeMinBedrooms_IsValid(int minBedrooms)
        {
            Assert.Empty(Validate(new ListingSearchQuery { MinBedrooms = minBedrooms }));
        }

        [Fact]
        public void Validate_NegativeMinBedrooms_IsInvalid()
        {
            var error = Assert.Single(Validate(new ListingSearchQuery { MinBedrooms = -1 }));
            Assert.Contains(nameof(ListingSearchQuery.MinBedrooms), error.MemberNames);
        }

        [Fact]
        public void Defaults_AreFirstPageOfTwenty()
        {
            var query = new ListingSearchQuery();

            Assert.Equal(1, query.Page);
            Assert.Equal(20, query.PageSize);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(5, 100)]
        public void Validate_ValidPaging_IsValid(int page, int pageSize)
        {
            Assert.Empty(Validate(new ListingSearchQuery { Page = page, PageSize = pageSize }));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_PageBelowOne_IsInvalid(int page)
        {
            var error = Assert.Single(Validate(new ListingSearchQuery { Page = page }));
            Assert.Contains(nameof(ListingSearchQuery.Page), error.MemberNames);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Validate_PageSizeOutOfRange_IsInvalid(int pageSize)
        {
            var error = Assert.Single(Validate(new ListingSearchQuery { PageSize = pageSize }));
            Assert.Contains(nameof(ListingSearchQuery.PageSize), error.MemberNames);
        }

        [Fact]
        public void Validate_KeywordAtMaxLength_IsValid()
        {
            Assert.Empty(Validate(new ListingSearchQuery { Keyword = new string('a', 100) }));
        }

        [Fact]
        public void Validate_KeywordTooLong_IsInvalid()
        {
            var error = Assert.Single(Validate(new ListingSearchQuery { Keyword = new string('a', 101) }));
            Assert.Contains(nameof(ListingSearchQuery.Keyword), error.MemberNames);
        }
    }
}
