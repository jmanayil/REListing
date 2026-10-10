namespace REListing.Repositories.Options
{
    public class ListingDataOptions
    {
        public const string SectionName = "ListingData";

        // Relative paths are resolved against the application's output directory.
        public string FilePath { get; set; } = string.Empty;
    }
}
