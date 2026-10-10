using Microsoft.AspNetCore.Mvc;
using REListing.Mappings;
using REListing.Services.Contracts;
using REListing.ViewModels;

namespace REListing.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingsController : ControllerBase
    {
        private readonly IListingService _listingService;

        public ListingsController(IListingService listingService)
        {
            _listingService = listingService;
        }

        // GET api/listings?city=Reston&minPrice=500000&maxPrice=600000&minBedrooms=3&keyword=pool&page=1&pageSize=20
        // All parameters optional; page defaults to 1 and pageSize to 20 (max 100).
        // Results are ordered by rank (highest first), then newest first.
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<RankedListingViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public ActionResult<PagedResponse<RankedListingViewModel>> GetAll([FromQuery] ListingSearchQuery query)
        {
            var result = _listingService.Search(query.ToCriteria());
            return Ok(result.ToViewModel());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ListingViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<ListingViewModel> GetById(string id)
        {
            var listing = _listingService.GetById(id);
            return listing is null ? NotFound() : Ok(listing.ToViewModel());
        }
    }
}
