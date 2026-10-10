using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using REListing.Services.Contracts;
using REListing.Services.Options;

namespace REListing.Services.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RankingOptions>(configuration.GetSection(RankingOptions.SectionName));

            services.AddScoped<IListingService, ListingService>();
            services.AddScoped<IListingRankingService, ListingRankingService>();

            return services;
        }
    }
}
