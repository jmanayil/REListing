using Microsoft.Extensions.Logging.Abstractions;
using REListing.Domain.Enums;
using REListing.Repositories.Options;

namespace REListing.Repositories.Tests
{
    public class JsonListingRepositoryTests : IDisposable
    {
        private const string SampleFilePath = "Data/sample_listings.json";

        private readonly List<string> _tempFiles = new();

        public void Dispose()
        {
            foreach (var file in _tempFiles)
            {
                File.Delete(file);
            }
        }

        private static JsonListingRepository CreateRepository(string filePath) =>
            new(Microsoft.Extensions.Options.Options.Create(new ListingDataOptions { FilePath = filePath }),
                NullLogger<JsonListingRepository>.Instance);

        private string WriteTempFile(string content)
        {
            var path = Path.Combine(Path.GetTempPath(), $"listings-{Guid.NewGuid():N}.json");
            File.WriteAllText(path, content);
            _tempFiles.Add(path);
            return path;
        }

        [Fact]
        public void GetAll_SampleFile_LoadsAllListings()
        {
            var repository = CreateRepository(SampleFilePath);

            Assert.Equal(12, repository.GetAll().Count);
        }

        [Fact]
        public void GetAll_SampleFile_MapsAllFields()
        {
            var listing = CreateRepository(SampleFilePath).GetAll().Single(l => l.Id == "A1");

            Assert.Equal("MLS_A", listing.Source);
            Assert.Equal("123 Main St, Apt 4B", listing.Address);
            Assert.Equal("Springfield", listing.City);
            Assert.Equal("VA", listing.State);
            Assert.Equal("22150", listing.Zip);
            Assert.Equal(450000m, listing.Price);
            Assert.Equal(2, listing.Bedrooms);
            Assert.Equal(1.5m, listing.Bathrooms);
            Assert.Equal(980, listing.Sqft);
            Assert.Equal(38.7893, listing.Latitude);
            Assert.Equal(-77.1873, listing.Longitude);
            Assert.Equal(new DateOnly(2026, 8, 29), listing.ListedDate);
            Assert.Equal(ListingStatus.Active, listing.Status);
            Assert.Equal("Bright top-floor condo near shops and transit. Pet friendly.", listing.Description);
        }

        [Fact]
        public void GetAll_SampleFile_MapsPendingStatus()
        {
            var listing = CreateRepository(SampleFilePath).GetAll().Single(l => l.Id == "A7");

            Assert.Equal(ListingStatus.Pending, listing.Status);
        }

        [Fact]
        public void GetAll_EmptyArray_ReturnsEmptyList()
        {
            var repository = CreateRepository(WriteTempFile("[]"));

            Assert.Empty(repository.GetAll());
        }

        [Fact]
        public void GetAll_NullJson_ReturnsEmptyList()
        {
            var repository = CreateRepository(WriteTempFile("null"));

            Assert.Empty(repository.GetAll());
        }

        [Theory]
        [InlineData("A1")]
        [InlineData("a1")]
        public void GetById_ExistingId_ReturnsListingIgnoringCase(string id)
        {
            var listing = CreateRepository(SampleFilePath).GetById(id);

            Assert.NotNull(listing);
            Assert.Equal("A1", listing.Id);
        }

        [Fact]
        public void GetById_UnknownId_ReturnsNull()
        {
            Assert.Null(CreateRepository(SampleFilePath).GetById("ZZZ"));
        }

        [Theory]
        [InlineData("Springfield")]
        [InlineData("SPRINGFIELD")]
        public void GetByCity_ReturnsOnlyListingsInCityIgnoringCase(string city)
        {
            var listings = CreateRepository(SampleFilePath).GetByCity(city);

            Assert.Equal(new[] { "A1", "B7", "A2", "B8" }, listings.Select(l => l.Id));
        }

        [Fact]
        public void GetByCity_UnknownCity_ReturnsEmpty()
        {
            Assert.Empty(CreateRepository(SampleFilePath).GetByCity("Atlantis"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_FilePathNotConfigured_Throws(string filePath)
        {
            Assert.Throws<InvalidOperationException>(() => CreateRepository(filePath));
        }

        [Fact]
        public void Constructor_FileMissing_ThrowsFileNotFound()
        {
            var missingPath = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

            Assert.Throws<FileNotFoundException>(() => CreateRepository(missingPath));
        }

        [Fact]
        public void Constructor_MalformedJson_Throws()
        {
            var path = WriteTempFile("[{ \"id\": \"A1\", ");

            Assert.Throws<InvalidOperationException>(() => CreateRepository(path));
        }

        [Fact]
        public void Constructor_UnknownStatus_Throws()
        {
            var path = WriteTempFile("[{ \"id\": \"X1\", \"status\": \"sold\" }]");

            Assert.Throws<InvalidOperationException>(() => CreateRepository(path));
        }
    }
}
