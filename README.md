# REListing

An ASP.NET Core 8 Web API for searching real-estate listings. Listings are loaded from a JSON file at startup and can be filtered, ranked, sorted and paged.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or newer SDK that can target `net8.0`)

## Getting started

From the repository root (the folder containing `REListing.sln`):

```bash
dotnet build
```

```bash
dotnet test
```

```bash
dotnet run --project src/REListing
```

The API listens on `http://localhost:5161` (and `https://localhost:7103` with the `https` launch profile). In Development, Swagger UI is available at `/swagger`.

Sample requests are in [src/REListing/REListing.http](src/REListing/REListing.http), which can be run from Visual Studio or VS Code.

## Project structure

```
REListing.sln
src/
  REListing/               Web API: controllers, view models, mappings, Program.cs, Data/
  REListing.Services/      Business logic: filtering, ranking, sorting, paging
  REListing.Repositories/  Data access: loads listings from the JSON file
  REListing.Domain/        Shared domain model (Listing) and enums (ListingStatus)
tests/
  REListing.Tests/               Controller, mapping and query-validation tests
  REListing.Services.Tests/      Search and ranking tests
  REListing.Repositories.Tests/  JSON loading and lookup tests
```

Dependencies flow one way: **API → Services → Repositories → Domain**. The API also references Repositories, since it is where all services are registered.

| Layer | Responsibility |
|---|---|
| API | HTTP concerns: query binding and validation, mapping domain models to view models, status codes. |
| Services | All search logic. Interfaces live in `Contracts/`. |
| Repositories | Deliberately minimal: return all listings, one by id, or those in a city. No other logic. |
| Domain | Plain classes with no framework or serialization dependencies. |

Each project exposes its own registration method (`AddRepositories`, `AddServices`) called from `Program.cs`.

## API

### `GET /api/listings`

Searches listings. All query parameters are optional.

| Parameter | Type | Description |
|---|---|---|
| `city` | string | Exact city name, case-insensitive. |
| `minPrice` | decimal | Minimum price, inclusive. Must be ≥ 0. |
| `maxPrice` | decimal | Maximum price, inclusive. Must be ≥ 0 and ≥ `minPrice`. |
| `minBedrooms` | int | Minimum bedrooms, inclusive. Must be ≥ 0. |
| `keyword` | string | Text matched anywhere in the description, case-insensitive, as one phrase. Max 100 characters. |
| `page` | int | 1-based page number. Default `1`. |
| `pageSize` | int | Items per page, 1–100. Default `20`. |

Invalid parameters return `400 Bad Request` with a validation problem body naming each error.

**Example:** `GET /api/listings?minPrice=450000&maxPrice=550000&pageSize=2`

```json
{
  "items": [
    {
      "id": "A1",
      "source": "MLS_A",
      "address": "123 Main St, Apt 4B",
      "city": "Springfield",
      "state": "VA",
      "zip": "22150",
      "price": 450000,
      "bedrooms": 2,
      "bathrooms": 1.5,
      "sqft": 980,
      "latitude": 38.7893,
      "longitude": -77.1873,
      "listedDate": "2026-08-29",
      "status": "active",
      "description": "Bright top-floor condo near shops and transit. Pet friendly.",
      "rank": 10
    },
    { "id": "B7", "...": "...", "rank": 9.8 }
  ],
  "page": 1,
  "pageSize": 2,
  "totalCount": 7,
  "totalPages": 4
}
```

- `totalCount` counts all listings matching the filters, not just the current page.
- A `page` past the last page returns an empty `items` list with the correct totals.

### `GET /api/listings/{id}`

Returns one listing by id (case-insensitive), or `404 Not Found`. This response has no `rank`.

## Ranking and sort order

When a search includes a **full price range** (`minPrice` and `maxPrice`) and/or **`minBedrooms`**, each result gets a `rank`. Results are ordered by:

1. `rank`, highest first
2. `listedDate`, newest first
3. `id`, so pages are stable

Without ranking criteria, `rank` is `null` and results are simply newest first.

The rank is the sum of the parts that apply, rounded to 2 decimal places:

- **Price** (only when both `minPrice` and `maxPrice` are given): `10` at `minPrice`, falling linearly to `0` at `maxPrice`.
  - Formula: `10 × (maxPrice − price) / (maxPrice − minPrice)`.
  - Example: in a 400k–500k range, 425k scores 7.5.
- **Bedrooms** (only when `minBedrooms` is given): `(bedrooms − minBedrooms) × BedroomWeight`.
  - With the default weight of 3, each bedroom above the minimum adds 3 points.

Example: 425k with 4 bedrooms, searched with 400k–500k and `minBedrooms=3`, ranks 7.5 + 3 = **10.5**.

Ranking lives in `ListingRankingService`. Filtering and ordering are in `ListingService.Search`.

## Configuration

Settings are in [src/REListing/appsettings.json](src/REListing/appsettings.json):

| Setting | Default | Description |
|---|---|---|
| `ListingData:FilePath` | `Data/sample_listings.json` | Listing data file. Relative paths resolve against the app's output folder. |
| `Ranking:BedroomWeight` | `3` | Points per bedroom above `minBedrooms`. Decimals allowed; `0` turns bedroom ranking off. |

Like any ASP.NET Core setting, these can be overridden per environment without rebuilding, for example with the environment variable `Ranking__BedroomWeight=5`.

The data file is loaded once at startup and kept in memory. If it is missing, malformed or contains an unknown `status`, the app fails to start with an error naming the problem.

## Data

[src/REListing/Data/sample_listings.json](src/REListing/Data/sample_listings.json) holds 12 sample listings from two sources (`MLS_A`, `MLS_B`). Several describe the same property with slightly different addresses (e.g. `123 Main St, Apt 4B` and `123 Main Street, Unit 4B`). They are currently returned as separate listings.

Valid `status` values are `active` and `pending`.

## Tests

The tests use xUnit with hand-written fakes (in each test project's `Fakes/` folder) instead of a mocking library.

```bash
dotnet test
```
