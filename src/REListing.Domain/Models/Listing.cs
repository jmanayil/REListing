using REListing.Domain.Enums;

namespace REListing.Domain.Models
{
    public class Listing
    {
        public string Id { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Zip { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Bedrooms { get; set; }

        public decimal Bathrooms { get; set; }

        public int Sqft { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public DateOnly ListedDate { get; set; }

        public ListingStatus Status { get; set; }

        public string? Description { get; set; }
    }
}
