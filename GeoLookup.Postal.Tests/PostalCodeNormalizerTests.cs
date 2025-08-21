using Xunit;
using GeoLookup.Postal;

namespace GeoLookup.Postal.Tests;

public class PostalCodeNormalizerTests
{
    [Theory]
    [InlineData("12345", "US", "12345")]
    [InlineData("12345-6789", "US", "12345")]
    [InlineData("90210", "US", "90210")]
    [InlineData("90210-1234", "US", "90210")]
    [InlineData("K1A 0B1", "CA", "K1A0B1")]
    [InlineData("k1a 0b1", "CA", "K1A0B1")]
    [InlineData("K1A0B1", "CA", "K1A0B1")]
    [InlineData("M5V 3L9", "CA", "M5V3L9")]
    public void Normalize_ValidPostalCodes_ReturnsNormalizedCode(string input, string countryCode, string expected)
    {
        var result = PostalCodeNormalizer.Normalize(input, countryCode);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1234", "US")] // Too short
    [InlineData("123456", "US")] // Too long without dash
    [InlineData("12345-123", "US")] // Invalid extension
    [InlineData("abcde", "US")] // Non-numeric
    [InlineData("K1A", "CA")] // Too short
    [InlineData("K1A 0B", "CA")] // Too short
    [InlineData("1A1 0B1", "CA")] // Invalid format
    [InlineData("K11 0B1", "CA")] // Invalid format
    [InlineData("", "US")]
    [InlineData("12345", "")] // Empty country code
    [InlineData(null, "US")]
    [InlineData("12345", null)]
    public void Normalize_InvalidPostalCodes_ReturnsNull(string input, string countryCode)
    {
        var result = PostalCodeNormalizer.Normalize(input, countryCode);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("12345", "US", true)]
    [InlineData("K1A 0B1", "CA", true)]
    [InlineData("invalid", "US", false)]
    [InlineData("invalid", "CA", false)]
    [InlineData("12345", "FR", false)] // Unsupported country
    public void IsValid_VariousInputs_ReturnsExpectedResult(string postalCode, string countryCode, bool expected)
    {
        var result = PostalCodeNormalizer.IsValid(postalCode, countryCode);
        Assert.Equal(expected, result);
    }
}
