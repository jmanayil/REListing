namespace REListing.Services.Models
{
    // One page of results plus the totals needed to page through the rest.
    public class PagedResult<T>
    {
        public PagedResult(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public IReadOnlyList<T> Items { get; }

        public int Page { get; }

        public int PageSize { get; }

        public int TotalCount { get; }

        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
