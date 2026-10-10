namespace REListing.ViewModels
{
    // Paged API response: one page of items plus totals for navigating the rest.
    public class PagedResponse<T>
    {
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public int TotalPages { get; set; }
    }
}
