using GeoLookup.Postal;
using GeoLookup.Postal.Tools;
using System.IO.Compression;

Console.WriteLine("GeoLookup.Postal Demo & Data Processor");
Console.WriteLine("=====================================");

var commandArgs = Environment.GetCommandLineArgs();

if (commandArgs.Length > 1 && commandArgs[1] == "download-and-process")
{
    await DownloadAndProcessData();
}
else
{
    RunDemo();
}

static void RunDemo()
{
    Console.WriteLine("\n1. Testing Postal Code Normalization:");
    
    var testCodes = new[]
    {
        ("90210", "US"),
        ("90210-1234", "US"),
        ("K1A 0B1", "CA"),
        ("k1a0b1", "CA"),
        ("invalid", "US"),
        ("123", "CA")
    };
    
    foreach (var (code, country) in testCodes)
    {
        var normalized = PostalCodeNormalizer.Normalize(code, country);
        var isValid = PostalCodeNormalizer.IsValid(code, country);
        Console.WriteLine($"  {code} ({country}) -> {normalized ?? "INVALID"} [Valid: {isValid}]");
    }
    
    Console.WriteLine("\n2. Testing Lookup Service:");
    
    try
    {
        using var lookup = new PostalCodeLookup();
        Console.WriteLine("✓ PostalCodeLookup initialized successfully");
        
        // Since we don't have real data yet, this will show the structure
        var result = lookup.Lookup("90210", "US");
        if (result.HasValue)
        {
            Console.WriteLine($"  Found: {result.Value.PlaceName} at {result.Value.Latitude}, {result.Value.Longitude}");
        }
        else
        {
            Console.WriteLine("  No data found (expected - run with 'download-and-process' to get real data)");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error: {ex.Message}");
        Console.WriteLine("  This is expected until real data is processed.");
    }
    
    Console.WriteLine("\nTo download and process real GeoNames data, run:");
    Console.WriteLine("  dotnet run download-and-process");
}

static async Task DownloadAndProcessData()
{
    Console.WriteLine("\nDownloading and processing GeoNames data...");
    
    var tempDir = Path.Combine(Path.GetTempPath(), "geonames-data");
    Directory.CreateDirectory(tempDir);
    
    try
    {
        // Download US data
        Console.WriteLine("Downloading US postal code data...");
        var usZipPath = Path.Combine(tempDir, "US.zip");
        await DataPreprocessor.DownloadGeoNamesDataAsync("US", usZipPath);
        
        // Extract US data
        var usExtractPath = Path.Combine(tempDir, "US");
        Directory.CreateDirectory(usExtractPath);
        ZipFile.ExtractToDirectory(usZipPath, usExtractPath);
        
        var usCsvPath = Path.Combine(usExtractPath, "US.txt");
        if (File.Exists(usCsvPath))
        {
            Console.WriteLine("Processing US data...");
            var usIndexPath = Path.Combine("../GeoLookup.Postal/Data", "USPostalCodeIndex.dat");
            var usDataPath = Path.Combine("../GeoLookup.Postal/Data", "USPostalCodeData.dat");
            await DataPreprocessor.ProcessCsvAsync(usCsvPath, usIndexPath, usDataPath, "US");
        }
        
        // Download CA data
        Console.WriteLine("Downloading CA postal code data...");
        var caZipPath = Path.Combine(tempDir, "CA.zip");
        await DataPreprocessor.DownloadGeoNamesDataAsync("CA", caZipPath);
        
        // Extract CA data
        var caExtractPath = Path.Combine(tempDir, "CA");
        Directory.CreateDirectory(caExtractPath);
        ZipFile.ExtractToDirectory(caZipPath, caExtractPath);
        
        var caCsvPath = Path.Combine(caExtractPath, "CA.txt");
        if (File.Exists(caCsvPath))
        {
            Console.WriteLine("Processing CA data...");
            var caIndexPath = Path.Combine("../GeoLookup.Postal/Data", "CAPostalCodeIndex.dat");
            var caDataPath = Path.Combine("../GeoLookup.Postal/Data", "CAPostalCodeData.dat");
            await DataPreprocessor.ProcessCsvAsync(caCsvPath, caIndexPath, caDataPath, "CA");
        }
        
        Console.WriteLine("✓ Data processing complete!");
        Console.WriteLine("Note: You'll need to manually combine the US and CA data files for the main package.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error during data processing: {ex.Message}");
    }
    finally
    {
        // Cleanup
        if (Directory.Exists(tempDir))
        {
            Directory.Delete(tempDir, true);
        }
    }
}
