using REListing.Domain.Models;
using REListing.Services.Options;

namespace REListing.Services.Tests
{
    public class ListingRankingServiceTests
    {
        private readonly ListingRankingService _service = CreateService(RankingOptions.DefaultBedroomWeight);

        private static ListingRankingService CreateService(decimal bedroomWeight) =>
            new(Microsoft.Extensions.Options.Options.Create(new RankingOptions { BedroomWeight = bedroomWeight }));

        private static Listing ListingWith(decimal price = 450000m, int bedrooms = 3) =>
            new() { Id = "X1", Price = price, Bedrooms = bedrooms };

        // --- Price ---

        [Theory]
        [InlineData(400000, 10.0)]   // at min
        [InlineData(425000, 7.5)]
        [InlineData(450000, 5.0)]    // midpoint
        [InlineData(475000, 2.5)]
        [InlineData(500000, 0.0)]    // at max
        public void Rank_PriceWithinRange_ScoresLinearlyFromMinToMax(int price, double expected)
        {
            Assert.Equal((decimal)expected, _service.Rank(ListingWith(price), 400000m, 500000m, null));
        }

        [Fact]
        public void Rank_PriceBelowMin_IsCappedAtTen()
        {
            Assert.Equal(10m, _service.Rank(ListingWith(350000m), 400000m, 500000m, null));
        }

        [Fact]
        public void Rank_PriceAboveMax_IsCappedAtZero()
        {
            Assert.Equal(0m, _service.Rank(ListingWith(550000m), 400000m, 500000m, null));
        }

        [Fact]
        public void Rank_MinEqualsMax_PriceAtThatValueScoresTen()
        {
            Assert.Equal(10m, _service.Rank(ListingWith(450000m), 450000m, 450000m, null));
        }

        [Fact]
        public void Rank_RoundsToTwoDecimals()
        {
            // 10 * (300000 - 100000) / 300000 = 6.666...
            Assert.Equal(6.67m, _service.Rank(ListingWith(100000m), 0m, 300000m, null));
        }

        [Theory]
        [InlineData(400000, null)]
        [InlineData(null, 500000)]
        public void Rank_PartialPriceRangeOnly_ReturnsNull(int? minPrice, int? maxPrice)
        {
            Assert.Null(_service.Rank(ListingWith(), minPrice, maxPrice, null));
        }

        // --- Bedrooms ---

        [Theory]
        [InlineData(3, 3, 0)]    // exactly the minimum
        [InlineData(4, 3, 3)]    // one extra bedroom
        [InlineData(4, 2, 6)]    // two extra bedrooms
        [InlineData(2, 3, -3)]   // one short (formula applied as-is)
        public void Rank_OnlyMinBedrooms_ScoresExtraBedroomsTimesDefaultWeight(int bedrooms, int minBedrooms, int expected)
        {
            Assert.Equal((decimal)expected, _service.Rank(ListingWith(bedrooms: bedrooms), null, null, minBedrooms));
        }

        [Fact]
        public void Rank_DefaultBedroomWeight_IsThree()
        {
            Assert.Equal(3m, new RankingOptions().BedroomWeight);
        }

        [Theory]
        [InlineData(5.0, 10.0)]   // 2 extra bedrooms * 5
        [InlineData(1.5, 3.0)]    // 2 extra bedrooms * 1.5
        [InlineData(0.0, 0.0)]    // bedrooms turned off
        public void Rank_UsesConfiguredBedroomWeight(double weight, double expected)
        {
            var service = CreateService((decimal)weight);

            Assert.Equal((decimal)expected, service.Rank(ListingWith(bedrooms: 4), null, null, 2));
        }

        // --- Combined ---

        [Fact]
        public void Rank_PriceRangeAndMinBedrooms_SumsBothScores()
        {
            // Price: 425k in 400k–500k = 7.5. Bedrooms: (4 - 3) * 3 = 3.
            Assert.Equal(10.5m, _service.Rank(ListingWith(425000m, bedrooms: 4), 400000m, 500000m, 3));
        }

        [Fact]
        public void Rank_PriceRangeAndMinBedroomsAtMinimum_EqualsPriceScore()
        {
            Assert.Equal(5m, _service.Rank(ListingWith(450000m, bedrooms: 3), 400000m, 500000m, 3));
        }

        [Fact]
        public void Rank_NoCriteria_ReturnsNull()
        {
            Assert.Null(_service.Rank(ListingWith(), null, null, null));
        }

        // --- Validation ---

        [Fact]
        public void Rank_MinPriceGreaterThanMaxPrice_Throws()
        {
            Assert.Throws<ArgumentException>(() => _service.Rank(ListingWith(), 500000m, 400000m, null));
        }

        [Fact]
        public void Rank_NullListing_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _service.Rank(null!, 400000m, 500000m, null));
        }
    }
}
