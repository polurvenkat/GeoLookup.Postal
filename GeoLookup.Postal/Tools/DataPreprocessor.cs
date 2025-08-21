using System.Globalization;
using System.Text;
using System.Text.Json;
using GeoLookup.Postal.Models;

namespace GeoLookup.Postal.Tools;

/// <summary>
/// Tool for preprocessing GeoNames postal code CSV data into binary format.
/// </summary>
public static class DataPreprocessor
{
    /// <summary>
    /// Processes GeoNames CSV data and creates binary index and data files.
    /// </summary>
    /// <param name="csvFilePath">Path to the GeoNames CSV file.</param>
    /// <param name="indexOutputPath">Path for the binary index output.</param>
    /// <param name="dataOutputPath">Path for the binary data output.</param>
    /// <param name="countryCode">Country code to filter (US or CA).</param>
    public static async Task ProcessCsvAsync(string csvFilePath, string indexOutputPath, 
        string dataOutputPath, string countryCode)
    {
        var locations = new List<PostalCodeLocation>();
        
        // Read and parse CSV
        using var reader = new StreamReader(csvFilePath);
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            var location = ParseCsvLine(line, countryCode);
            if (location.HasValue)
            {
                locations.Add(location.Value);
            }
        }
        
        // Sort by postal code for binary search
        locations.Sort((x, y) => string.Compare(x.PostalCode, y.PostalCode, StringComparison.Ordinal));
        
        // Write binary files
        await WriteBinaryFilesAsync(locations, indexOutputPath, dataOutputPath);
        
        Console.WriteLine($"Processed {locations.Count} postal codes for {countryCode}");
    }
    
    private static PostalCodeLocation? ParseCsvLine(string csvLine, string countryCode)
    {
        try
        {
            // GeoNames postal code format: country code, postal code, place name, admin name1, admin code1, admin name2, admin code2, admin name3, admin code3, latitude, longitude, accuracy
            var fields = csvLine.Split('\t');
            if (fields.Length < 10) return null;
            
            var lineCountryCode = fields[0];
            if (!string.Equals(lineCountryCode, countryCode, StringComparison.OrdinalIgnoreCase))
                return null;
                
            var rawPostalCode = fields[1];
            var normalizedCode = PostalCodeNormalizer.Normalize(rawPostalCode, countryCode);
            if (normalizedCode == null) return null;
            
            var placeName = fields[2];
            var adminCode1 = fields[4]; // State/Province code
            var adminCode2 = fields[6]; // County/District code
            
            if (!double.TryParse(fields[9], NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude) ||
                !double.TryParse(fields[10], NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude))
                return null;
            
            return new PostalCodeLocation(
                normalizedCode,
                latitude,
                longitude,
                placeName,
                adminCode1,
                adminCode2,
                countryCode.ToUpperInvariant()
            );
        }
        catch
        {
            return null;
        }
    }
    
    private static async Task WriteBinaryFilesAsync(List<PostalCodeLocation> locations, 
        string indexOutputPath, string dataOutputPath)
    {
        var indexEntries = new List<PostalCodeIndexEntry>();
        
        // Write data file and build index
        using (var dataWriter = new FileStream(dataOutputPath, FileMode.Create))
        {
            foreach (var location in locations)
            {
                var offset = dataWriter.Position;
                var json = JsonSerializer.Serialize(location);
                var bytes = Encoding.UTF8.GetBytes(json);
                
                await dataWriter.WriteAsync(bytes);
                
                indexEntries.Add(new PostalCodeIndexEntry(location.PostalCode, offset, bytes.Length));
            }
        }
        
        // Write index file
        using var indexWriter = new BinaryWriter(new FileStream(indexOutputPath, FileMode.Create));
        indexWriter.Write(indexEntries.Count);
        
        foreach (var entry in indexEntries)
        {
            indexWriter.Write(entry.PostalCode);
            indexWriter.Write(entry.Offset);
            indexWriter.Write(entry.Length);
        }
    }
    
    /// <summary>
    /// Downloads GeoNames postal code data for the specified country.
    /// </summary>
    /// <param name="countryCode">Country code (US or CA).</param>
    /// <param name="outputPath">Path to save the downloaded file.</param>
    public static async Task DownloadGeoNamesDataAsync(string countryCode, string outputPath)
    {
        var url = countryCode.ToUpperInvariant() switch
        {
            "US" => "http://download.geonames.org/export/zip/US.zip",
            "CA" => "http://download.geonames.org/export/zip/CA.zip",
            _ => throw new ArgumentException($"Unsupported country code: {countryCode}")
        };
        
        using var client = new HttpClient();
        using var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        await using var fileStream = new FileStream(outputPath, FileMode.Create);
        await response.Content.CopyToAsync(fileStream);
        
        Console.WriteLine($"Downloaded {countryCode} data to {outputPath}");
    }
}
