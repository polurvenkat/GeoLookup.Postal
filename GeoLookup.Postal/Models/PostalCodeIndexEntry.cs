namespace GeoLookup.Postal.Models;

/// <summary>
/// Represents an index entry for binary search optimization.
/// </summary>
internal readonly record struct PostalCodeIndexEntry
{
    /// <summary>
    /// The normalized postal code for indexing.
    /// </summary>
    public string PostalCode { get; init; }

    /// <summary>
    /// The offset in the data file where the full record is stored.
    /// </summary>
    public long Offset { get; init; }

    /// <summary>
    /// The length of the record in bytes.
    /// </summary>
    public int Length { get; init; }

    public PostalCodeIndexEntry(string postalCode, long offset, int length)
    {
        PostalCode = postalCode;
        Offset = offset;
        Length = length;
    }
}
