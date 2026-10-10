using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using REListing.Repositories.Contracts;
using REListing.Repositories.Options;

namespace REListing.Repositories.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ListingDataOptions>(configuration.GetSection(ListingDataOptions.SectionName));

            // Singleton: the data is loaded once and never changes.
            services.AddSingleton<IListingRepository, JsonListingRepository>();

            return services;
        }
    }
}
