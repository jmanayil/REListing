namespace REListing.Services.Models
{
    // Search filters for listings. Any filter left null (or blank, for text) is not applied.
    public class ListingSearchCriteria
    {
        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public int? MinBedrooms { get; set; }

        // Exact city name, case-insensitive.
        public string? City { get; set; }

        // Case-insensitive text matched anywhere in the description.
        public string? Keyword { get; set; }

        // 1-based page number.
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;
    }
}
