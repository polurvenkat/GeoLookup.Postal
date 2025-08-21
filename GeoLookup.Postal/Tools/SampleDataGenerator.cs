using System.Text;
using System.Text.Json;
using GeoLookup.Postal.Models;

namespace GeoLookup.Postal.Tools;

/// <summary>
/// Utility class to create sample data for testing purposes.
/// </summary>
public static class SampleDataGenerator
{
    public static void CreateSampleData(string indexPath, string dataPath)
    {
        var sampleLocations = new[]
        {
            new PostalCodeLocation("10001", 40.7505, -73.9971, "New York", "NY", "061", "US"),
            new PostalCodeLocation("90210", 34.0901, -118.4065, "Beverly Hills", "CA", "06", "US"),
            new PostalCodeLocation("K1A0B1", 45.4215, -75.6972, "Ottawa", "ON", "", "CA"),
            new PostalCodeLocation("M5V3L9", 43.6426, -79.3871, "Toronto", "ON", "", "CA")
        };

        // Sort by postal code for binary search
        var sortedLocations = sampleLocations.OrderBy(l => l.PostalCode, StringComparer.Ordinal).ToArray();

        // Create index and data files
        using var indexWriter = new BinaryWriter(File.Create(indexPath));
        using var dataFile = File.Create(dataPath);
        
        indexWriter.Write(sortedLocations.Length);
        
        long currentOffset = 0;
        
        foreach (var location in sortedLocations)
        {
            var json = JsonSerializer.Serialize(location);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            
            // Write to index
            indexWriter.Write(location.PostalCode);
            indexWriter.Write(currentOffset);
            indexWriter.Write(jsonBytes.Length);
            
            // Write to data file
            dataFile.Write(jsonBytes);
            currentOffset += jsonBytes.Length;
        }
    }
}
