using REListing.Domain.Enums;
using REListing.Domain.Models;
using REListing.Services.Tests.Fakes;

namespace REListing.Services.Tests
{
    public class ListingServiceTests
    {
        private static readonly List<Listing> Listings = new()
        {
            new Listing { Id = "A1", City = "Springfield", Status = ListingStatus.Active },
            new Listing { Id = "B7", City = "Fairfax", Status = ListingStatus.Pending }
        };

        private readonly FakeListingRepository _repository = new(Listings);
        private readonly ListingService _service;

        public ListingServiceTests()
        {
            _service = new ListingService(_repository, new FakeListingRankingService());
        }

        [Fact]
        public void GetAll_ReturnsAllListingsFromRepository()
        {
            Assert.Equal(new[] { "A1", "B7" }, _service.GetAll().Select(l => l.Id));
        }

        [Fact]
        public void GetById_ExistingId_ReturnsListing()
        {
            var listing = _service.GetById("B7");

            Assert.NotNull(listing);
            Assert.Equal("B7", listing.Id);
        }

        [Fact]
        public void GetById_IdWithSurroundingWhitespace_TrimsBeforeLookup()
        {
            var listing = _service.GetById("  A1 ");

            Assert.NotNull(listing);
            Assert.Equal(new[] { "A1" }, _repository.RequestedIds);
        }

        [Fact]
        public void GetById_UnknownId_ReturnsNull()
        {
            Assert.Null(_service.GetById("ZZZ"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GetById_BlankId_ReturnsNullWithoutQueryingRepository(string id)
        {
            Assert.Null(_service.GetById(id));
            Assert.Empty(_repository.RequestedIds);
        }
    }
}
