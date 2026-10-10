using REListing.Domain.Models;
using REListing.Repositories.Contracts;

namespace REListing.Services.Tests.Fakes
{
    public class FakeListingRepository : IListingRepository
    {
        private readonly IReadOnlyList<Listing> _listings;

        public FakeListingRepository(IReadOnlyList<Listing> listings) => _listings = listings;

        // Ids passed to GetById, so tests can check what reached the repository.
        public List<string> RequestedIds { get; } = new();

        // Cities passed to GetByCity.
        public List<string> RequestedCities { get; } = new();

        public int GetAllCallCount { get; private set; }

        public IReadOnlyList<Listing> GetAll()
        {
            GetAllCallCount++;
            return _listings;
        }

        public Listing? GetById(string id)
        {
            RequestedIds.Add(id);
            return _listings.FirstOrDefault(l => l.Id == id);
        }

        public IReadOnlyList<Listing> GetByCity(string city)
        {
            RequestedCities.Add(city);
            return _listings.Where(l => string.Equals(l.City, city, StringComparison.OrdinalIgnoreCase)).ToList();
        }
    }
}
