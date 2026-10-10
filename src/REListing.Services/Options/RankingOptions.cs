namespace REListing.Services.Options
{
    // Tunable ranking weights, bound from the "Ranking" section of appsettings.
    public class RankingOptions
    {
        public const string SectionName = "Ranking";

        public const decimal DefaultBedroomWeight = 3m;

        // Points per bedroom above the requested minimum.
        public decimal BedroomWeight { get; set; } = DefaultBedroomWeight;
    }
}
