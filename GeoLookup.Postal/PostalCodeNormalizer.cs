using System.Text.RegularExpressions;

namespace GeoLookup.Postal;

/// <summary>
/// Utility class for normalizing postal codes according to country-specific rules.
/// </summary>
public static class PostalCodeNormalizer
{
    private static readonly Regex UsPostalCodeRegex = new(@"^(\d{5})(-\d{4})?$", RegexOptions.Compiled);
    private static readonly Regex CaPostalCodeRegex = new(@"^([A-Za-z]\d[A-Za-z])\s*(\d[A-Za-z]\d)$", RegexOptions.Compiled);

    /// <summary>
    /// Detects the country code based on postal code format.
    /// </summary>
    /// <param name="postalCode">The postal code to analyze.</param>
    /// <returns>The detected country code (US or CA), or null if format is not recognized.</returns>
    public static string? DetectCountry(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return null;

        var trimmed = postalCode.Trim();
        
        // Canadian postal codes: Letter-Digit-Letter Digit-Letter-Digit (with optional space)
        if (CaPostalCodeRegex.IsMatch(trimmed))
            return "CA";
            
        // US postal codes: 5 digits or 5 digits + dash + 4 digits
        if (UsPostalCodeRegex.IsMatch(trimmed))
            return "US";
            
        return null;
    }

    /// <summary>
    /// Normalizes a postal code by automatically detecting the country format.
    /// </summary>
    /// <param name="postalCode">The raw postal code input.</param>
    /// <returns>A tuple containing the normalized postal code and detected country code, or null if invalid.</returns>
    public static (string NormalizedCode, string CountryCode)? Normalize(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return null;

        var countryCode = DetectCountry(postalCode);
        if (countryCode == null)
            return null;

        var normalizedCode = Normalize(postalCode, countryCode);
        if (normalizedCode == null)
            return null;

        return (normalizedCode, countryCode);
    }

    /// <summary>
    /// Normalizes a postal code based on the country code.
    /// </summary>
    /// <param name="postalCode">The raw postal code input.</param>
    /// <param name="countryCode">The country code (US or CA).</param>
    /// <returns>The normalized postal code, or null if invalid.</returns>
    public static string? Normalize(string postalCode, string countryCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode) || string.IsNullOrWhiteSpace(countryCode))
            return null;

        return countryCode.ToUpperInvariant() switch
        {
            "US" => NormalizeUs(postalCode.Trim()),
            "CA" => NormalizeCa(postalCode.Trim()),
            _ => null
        };
    }

    /// <summary>
    /// Normalizes US postal codes to 5-digit format (strips -#### extensions).
    /// </summary>
    /// <param name="postalCode">The US postal code.</param>
    /// <returns>The normalized 5-digit postal code.</returns>
    public static string? NormalizeUs(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return null;

        var match = UsPostalCodeRegex.Match(postalCode.Trim());
        return match.Success ? match.Groups[1].Value : null;
    }

    /// <summary>
    /// Normalizes Canadian postal codes to uppercase without spaces (e.g., K1A 0B1 → K1A0B1).
    /// </summary>
    /// <param name="postalCode">The Canadian postal code.</param>
    /// <returns>The normalized Canadian postal code.</returns>
    public static string? NormalizeCa(string postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            return null;

        var match = CaPostalCodeRegex.Match(postalCode.Trim());
        return match.Success ? $"{match.Groups[1].Value}{match.Groups[2].Value}".ToUpperInvariant() : null;
    }

    /// <summary>
    /// Validates if a postal code is in a recognized format (auto-detects country).
    /// </summary>
    /// <param name="postalCode">The postal code to validate.</param>
    /// <returns>True if the postal code is valid for any supported country.</returns>
    public static bool IsValid(string postalCode)
    {
        return Normalize(postalCode) != null;
    }

    /// <summary>
    /// Validates if a postal code is in the correct format for the specified country.
    /// </summary>
    /// <param name="postalCode">The postal code to validate.</param>
    /// <param name="countryCode">The country code (US or CA).</param>
    /// <returns>True if the postal code is valid for the country.</returns>
    public static bool IsValid(string postalCode, string countryCode)
    {
        return Normalize(postalCode, countryCode) != null;
    }
}
