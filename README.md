# GeoLookup.Postal

High-performance postal code geolocation library for .NET 9+ using GeoNames data with binary search optimization.

## Features

- **Fast Lookups**: O(log n) binary search for postal code lookups
- **US & Canada Support**: Covers all postal codes for United States and Canada
- **Normalization**: Automatic postal code normalization (US: 5-digit format, CA: uppercase without spaces)
- **Batch Operations**: Efficient batch lookups for multiple postal codes
- **Radius Search**: Find postal codes within a specified distance
- **Embedded Data**: No external dependencies or database required
- **Thread Safe**: Safe for concurrent access

## Data Source

This package uses postal code data from GeoNames (http://www.geonames.org/), which is licensed under the Creative Commons Attribution 4.0 License. The data includes:

- Postal codes for United States and Canada
- Geographic coordinates (latitude/longitude centroids)
- Place names and administrative divisions
- High accuracy for proximity searches and mapping

## Attribution

Postal code data © GeoNames contributors, licensed under Creative Commons Attribution 4.0 (CC BY 4.0).
See: https://creativecommons.org/licenses/by/4.0/

## Installation

```bash
dotnet add package GeoLookup.Postal
```

## Quick Start

```csharp
using GeoLookup.Postal;

// Create lookup service
using var lookup = new PostalCodeLookup();

// Lookup a US postal code
var location = lookup.Lookup("90210", "US");
if (location.HasValue)
{
    Console.WriteLine($"Location: {location.Value.PlaceName}");
    Console.WriteLine($"Coordinates: {location.Value.Latitude}, {location.Value.Longitude}");
}

// Lookup a Canadian postal code
var caLocation = lookup.Lookup("K1A 0B1", "CA");
if (caLocation.HasValue)
{
    Console.WriteLine($"Location: {caLocation.Value.PlaceName}");
    Console.WriteLine($"Coordinates: {caLocation.Value.Latitude}, {caLocation.Value.Longitude}");
}

// Batch lookup
var requests = new[]
{
    ("10001", "US"),
    ("90210", "US"),
    ("K1A 0B1", "CA"),
    ("M5V 3L9", "CA")
};

var results = lookup.LookupBatch(requests);
foreach (var result in results)
{
    Console.WriteLine($"{result.Key}: {result.Value?.PlaceName ?? "Not found"}");
}

// Find postal codes within radius
var nearby = lookup.FindWithinRadius(40.7128, -74.0060, 10); // 10km around NYC
foreach (var location in nearby.Take(5))
{
    Console.WriteLine($"{location.PostalCode}: {location.PlaceName}");
}
```

## API Reference

### PostalCodeLookup

Main service class for postal code lookups.

#### Methods

- `Lookup(string postalCode, string countryCode)`: Look up a single postal code
- `LookupBatch(IEnumerable<(string, string)> requests)`: Batch lookup for multiple postal codes
- `FindWithinRadius(double lat, double lon, double radiusKm, string? countryCode = null)`: Find postal codes within radius

### PostalCodeNormalizer

Utility class for postal code normalization and validation.

#### Methods

- `Normalize(string postalCode, string countryCode)`: Normalize postal code format
- `IsValid(string postalCode, string countryCode)`: Validate postal code format
- `NormalizeUs(string postalCode)`: US-specific normalization
- `NormalizeCa(string postalCode)`: Canada-specific normalization

### PostalCodeLocation

Result structure containing location data.

#### Properties

- `PostalCode`: Normalized postal code
- `Latitude`: Geographic latitude
- `Longitude`: Geographic longitude
- `PlaceName`: Associated place name
- `AdminCode1`: State/province code
- `AdminCode2`: County/district code
- `CountryCode`: Country code (US or CA)

## Postal Code Formats

### United States
- Input: `12345` or `12345-6789`
- Normalized: `12345` (5-digit format)

### Canada
- Input: `K1A 0B1` or `K1A0B1`
- Normalized: `K1A0B1` (uppercase, no spaces)

## Performance

- **Lookup Time**: O(log n) binary search ~1-5 microseconds per lookup
- **Memory Usage**: ~50-100MB for full US+CA dataset
- **Package Size**: ~25-40MB with compressed binary data
- **Thread Safety**: Yes, safe for concurrent access

## Building from Source

```bash
git clone <repository-url>
cd GeoLookup.Postal
dotnet restore
dotnet build
dotnet test
```

## License

This package is licensed under the Creative Commons Attribution 4.0 License (CC BY 4.0) to comply with the GeoNames data license.

## Contributing

Contributions are welcome! Please ensure all tests pass and follow the existing code style.

## Changelog

### 1.0.0
- Initial release
- US and Canada postal code support
- Binary search optimization
- Batch lookup support
- Radius search functionality
