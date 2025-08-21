using System.Reflection;
using System.Text;
using System.Text.Json;
using GeoLookup.Postal.Models;

namespace GeoLookup.Postal;

/// <summary>
/// High-performance postal code lookup service using binary search optimization.
/// </summary>
public class PostalCodeLookup : IDisposable
{
    private readonly PostalCodeIndexEntry[] _index;
    private readonly Stream _dataStream;
    private readonly bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of the PostalCodeLookup class.
    /// </summary>
    public PostalCodeLookup()
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        // Load the binary index
        using var indexStream = assembly.GetManifestResourceStream("GeoLookup.Postal.Data.PostalCodeIndex.dat")
            ?? throw new InvalidOperationException("Index data not found in embedded resources.");
        
        _index = LoadIndex(indexStream);
        
        // Keep data stream open for lookups
        _dataStream = assembly.GetManifestResourceStream("GeoLookup.Postal.Data.PostalCodeData.dat")
            ?? throw new InvalidOperationException("Data file not found in embedded resources.");
    }

    /// <summary>
    /// Looks up a postal code with automatic country detection.
    /// </summary>
    /// <param name="postalCode">The postal code to look up.</param>
    /// <returns>The postal code location, or null if not found or format is not recognized.</returns>
    public PostalCodeLocation? Lookup(string postalCode)
    {
        var normalized = PostalCodeNormalizer.Normalize(postalCode);
        if (normalized == null)
            return null;

        return Lookup(normalized.Value.NormalizedCode, normalized.Value.CountryCode);
    }

    /// <summary>
    /// Looks up a postal code and returns the geographic location.
    /// </summary>
    /// <param name="postalCode">The postal code to look up.</param>
    /// <param name="countryCode">The country code (US or CA).</param>
    /// <returns>The postal code location, or null if not found.</returns>
    public PostalCodeLocation? Lookup(string postalCode, string countryCode)
    {
        var normalizedCode = PostalCodeNormalizer.Normalize(postalCode, countryCode);
        if (normalizedCode == null)
            return null;

        // Binary search for the postal code in the index
        var index = Array.BinarySearch(_index, new PostalCodeIndexEntry(normalizedCode, 0, 0), 
            Comparer<PostalCodeIndexEntry>.Create((x, y) => string.Compare(x.PostalCode, y.PostalCode, StringComparison.Ordinal)));

        if (index < 0)
            return null;

        var entry = _index[index];
        return ReadLocationFromData(entry);
    }

    /// <summary>
    /// Looks up multiple postal codes in batch with automatic country detection.
    /// </summary>
    /// <param name="postalCodes">Collection of postal codes to look up.</param>
    /// <returns>Dictionary with results, keyed by original input postal code.</returns>
    public Dictionary<string, PostalCodeLocation?> LookupBatch(IEnumerable<string> postalCodes)
    {
        var results = new Dictionary<string, PostalCodeLocation?>();
        
        foreach (var postalCode in postalCodes)
        {
            results[postalCode] = Lookup(postalCode);
        }
        
        return results;
    }

    /// <summary>
    /// Looks up multiple postal codes in batch for better performance.
    /// </summary>
    /// <param name="requests">Collection of postal code and country code pairs.</param>
    /// <returns>Dictionary with results, keyed by original input postal code.</returns>
    public Dictionary<string, PostalCodeLocation?> LookupBatch(IEnumerable<(string PostalCode, string CountryCode)> requests)
    {
        var results = new Dictionary<string, PostalCodeLocation?>();
        
        foreach (var (postalCode, countryCode) in requests)
        {
            results[postalCode] = Lookup(postalCode, countryCode);
        }
        
        return results;
    }

    /// <summary>
    /// Gets all postal codes within a specified radius of a given location.
    /// </summary>
    /// <param name="latitude">The center latitude.</param>
    /// <param name="longitude">The center longitude.</param>
    /// <param name="radiusKm">The radius in kilometers.</param>
    /// <param name="countryCode">The country code to search within (optional).</param>
    /// <returns>Collection of postal codes within the radius.</returns>
    public IEnumerable<PostalCodeLocation> FindWithinRadius(double latitude, double longitude, double radiusKm, string? countryCode = null)
    {
        // For now, we'll iterate through all entries. In a production system,
        // you might want to implement spatial indexing for better performance.
        var results = new List<PostalCodeLocation>();
        
        foreach (var entry in _index)
        {
            var location = ReadLocationFromData(entry);
            if (location == null) continue;
            
            if (countryCode != null && !string.Equals(location.Value.CountryCode, countryCode, StringComparison.OrdinalIgnoreCase))
                continue;
                
            var distance = CalculateDistance(latitude, longitude, location.Value.Latitude, location.Value.Longitude);
            if (distance <= radiusKm)
            {
                results.Add(location.Value);
            }
        }
        
        return results.OrderBy(r => CalculateDistance(latitude, longitude, r.Latitude, r.Longitude));
    }

    private PostalCodeIndexEntry[] LoadIndex(Stream indexStream)
    {
        using var reader = new BinaryReader(indexStream);
        var count = reader.ReadInt32();
        var index = new PostalCodeIndexEntry[count];
        
        for (int i = 0; i < count; i++)
        {
            var postalCode = reader.ReadString();
            var offset = reader.ReadInt64();
            var length = reader.ReadInt32();
            index[i] = new PostalCodeIndexEntry(postalCode, offset, length);
        }
        
        return index;
    }

    private PostalCodeLocation? ReadLocationFromData(PostalCodeIndexEntry entry)
    {
        try
        {
            _dataStream.Seek(entry.Offset, SeekOrigin.Begin);
            var buffer = new byte[entry.Length];
            _dataStream.Read(buffer, 0, entry.Length);
            var json = Encoding.UTF8.GetString(buffer);
            return JsonSerializer.Deserialize<PostalCodeLocation>(json);
        }
        catch
        {
            return null;
        }
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    public void Dispose()
    {
        if (!_disposed)
        {
            _dataStream?.Dispose();
        }
    }
}
