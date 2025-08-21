# GeoLookup.Postal

High-performance postal code geolocation library for .NET 9+ using GeoNames data with binary search optimization.

## Features

- **Fast Lookups**: O(log n) binary search for postal code lookups
- **US & Canada Support**: Covers all postal codes for United States and Canada
- **Auto-Detection**: Automatically detects country from postal code format - no country code required!
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

// 🚀 NEW: Auto-detection - No country code required!
var location = lookup.Lookup("90210");  // Automatically detects US format
if (location.HasValue)
{
    Console.WriteLine($"Location: {location.Value.PlaceName}");
    Console.WriteLine($"Coordinates: {location.Value.Latitude}, {location.Value.Longitude}");
    Console.WriteLine($"Country: {location.Value.CountryCode}");
}

var caLocation = lookup.Lookup("K1A 0B1");  // Automatically detects Canadian format
if (caLocation.HasValue)
{
    Console.WriteLine($"Location: {caLocation.Value.PlaceName}");
    Console.WriteLine($"Country: {caLocation.Value.CountryCode}");
}

// Traditional method (still supported)
var traditionalLookup = lookup.Lookup("90210", "US");

// Auto-detection normalization
var normalized = PostalCodeNormalizer.Normalize("k1a 0b1");  // Returns ("K1A0B1", "CA")
if (normalized.HasValue)
{
    Console.WriteLine($"Normalized: {normalized.Value.NormalizedCode} ({normalized.Value.CountryCode})");
}

// Country detection
var country = PostalCodeNormalizer.DetectCountry("90210");  // Returns "US"
var country2 = PostalCodeNormalizer.DetectCountry("K1A 0B1");  // Returns "CA"

// Batch lookup with auto-detection
var postalCodes = new[] { "10001", "90210", "K1A 0B1", "M5V 3L9" };
var results = lookup.LookupBatch(postalCodes);
foreach (var result in results)
{
    Console.WriteLine($"{result.Key}: {result.Value?.PlaceName ?? "Not found"}");
}

// Traditional batch lookup (still supported)
var requests = new[]
{
    ("10001", "US"),
    ("90210", "US"),
    ("K1A 0B1", "CA"),
    ("M5V 3L9", "CA")
};
var traditionalResults = lookup.LookupBatch(requests);

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

- `Lookup(string postalCode)`: 🆕 Look up a postal code with automatic country detection
- `Lookup(string postalCode, string countryCode)`: Look up a single postal code with explicit country
- `LookupBatch(IEnumerable<string> postalCodes)`: 🆕 Batch lookup with auto-detection
- `LookupBatch(IEnumerable<(string, string)> requests)`: Batch lookup for multiple postal codes with explicit countries
- `FindWithinRadius(double lat, double lon, double radiusKm, string? countryCode = null)`: Find postal codes within radius

### PostalCodeNormalizer

Utility class for postal code normalization and validation.

#### Methods

- `DetectCountry(string postalCode)`: 🆕 Automatically detect country from postal code format
- `Normalize(string postalCode)`: 🆕 Normalize postal code with automatic country detection
- `IsValid(string postalCode)`: 🆕 Validate postal code format with auto-detection
- `Normalize(string postalCode, string countryCode)`: Normalize postal code format with explicit country
- `IsValid(string postalCode, string countryCode)`: Validate postal code format for specific country
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
