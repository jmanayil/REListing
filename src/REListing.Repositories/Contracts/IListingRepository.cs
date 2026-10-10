using REListing.Domain.Models;

namespace REListing.Repositories.Contracts
{
    public interface IListingRepository
    {
        IReadOnlyList<Listing> GetAll();

        Listing? GetById(string id);

        IReadOnlyList<Listing> GetByCity(string city);
    }
}
