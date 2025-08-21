namespace GeoLookup.Postal.Models;

/// <summary>
/// Represents a postal code location with geographic coordinates.
/// </summary>
public readonly record struct PostalCodeLocation
{
    /// <summary>
    /// The normalized postal code.
    /// </summary>
    public string PostalCode { get; init; }

    /// <summary>
    /// The latitude coordinate.
    /// </summary>
    public double Latitude { get; init; }

    /// <summary>
    /// The longitude coordinate.
    /// </summary>
    public double Longitude { get; init; }

    /// <summary>
    /// The place name associated with this postal code.
    /// </summary>
    public string PlaceName { get; init; }

    /// <summary>
    /// The administrative division (state/province).
    /// </summary>
    public string AdminCode1 { get; init; }

    /// <summary>
    /// The administrative subdivision (county/district).
    /// </summary>
    public string AdminCode2 { get; init; }

    /// <summary>
    /// The country code (US or CA).
    /// </summary>
    public string CountryCode { get; init; }

    public PostalCodeLocation(string postalCode, double latitude, double longitude, 
        string placeName, string adminCode1, string adminCode2, string countryCode)
    {
        PostalCode = postalCode;
        Latitude = latitude;
        Longitude = longitude;
        PlaceName = placeName;
        AdminCode1 = adminCode1;
        AdminCode2 = adminCode2;
        CountryCode = countryCode;
    }
}
