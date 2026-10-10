using System.ComponentModel.DataAnnotations;

namespace REListing.ViewModels
{
    // Optional query-string filters and paging for GET /api/listings. Omitted filters are not applied.
    public class ListingSearchQuery : IValidatableObject
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;

        public string? City { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "minPrice cannot be negative.")]
        public decimal? MinPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "maxPrice cannot be negative.")]
        public decimal? MaxPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "minBedrooms cannot be negative.")]
        public int? MinBedrooms { get; set; }

        // Free text matched anywhere in the description, case-insensitive.
        [StringLength(100, ErrorMessage = "keyword cannot be longer than 100 characters.")]
        public string? Keyword { get; set; }

        // 1-based page number.
        [Range(1, int.MaxValue, ErrorMessage = "page must be 1 or greater.")]
        public int Page { get; set; } = 1;

        [Range(1, MaxPageSize, ErrorMessage = "pageSize must be between 1 and 100.")]
        public int PageSize { get; set; } = DefaultPageSize;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinPrice > MaxPrice)
            {
                yield return new ValidationResult(
                    "minPrice cannot be greater than maxPrice.",
                    new[] { nameof(MinPrice), nameof(MaxPrice) });
            }
        }
    }
}
