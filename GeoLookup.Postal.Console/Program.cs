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
else if (commandArgs.Length > 1 && commandArgs[1] == "create-sample-data")
{
    CreateSampleData();
}
else
{
    RunDemo();
}

static void RunDemo()
{
    Console.WriteLine("\n1. Testing Postal Code Auto-Detection & Normalization:");
    
    var testCodes = new[]
    {
        "90210",          // US ZIP
        "90210-1234",     // US ZIP+4
        "K1A 0B1",        // Canadian postal code with space
        "k1a0b1",         // Canadian postal code without space, lowercase
        "T2X 1V4",        // Another Canadian postal code
        "invalid",        // Invalid format
        "123",            // Too short
        "A1A 1A1"         // Canadian format
    };
    
    Console.WriteLine("Auto-detection (no country code required):");
    foreach (var code in testCodes)
    {
        var country = PostalCodeNormalizer.DetectCountry(code);
        var normalized = PostalCodeNormalizer.Normalize(code);
        var isValid = PostalCodeNormalizer.IsValid(code);
        
        if (normalized.HasValue)
        {
            Console.WriteLine($"  {code} -> {normalized.Value.NormalizedCode} ({normalized.Value.CountryCode}) [Valid: {isValid}]");
        }
        else
        {
            Console.WriteLine($"  {code} -> INVALID [Valid: {isValid}]");
        }
    }
    
    Console.WriteLine("\nTraditional method (with country code):");
    var traditionalTests = new[]
    {
        ("90210", "US"),
        ("90210-1234", "US"),
        ("K1A 0B1", "CA"),
        ("k1a0b1", "CA"),
        ("invalid", "US"),
        ("123", "CA")
    };
    
    foreach (var (code, country) in traditionalTests)
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
        
        Console.WriteLine("\nTesting auto-detection lookup (no country code required):");
        var autoTestCodes = new[] { "90210", "K1A 0B1", "invalid" };
        
        foreach (var code in autoTestCodes)
        {
            var autoResult = lookup.Lookup(code);
            if (autoResult.HasValue)
            {
                Console.WriteLine($"  {code} -> Found: {autoResult.Value.PlaceName} at {autoResult.Value.Latitude}, {autoResult.Value.Longitude}");
            }
            else
            {
                Console.WriteLine($"  {code} -> Not found");
            }
        }
        
        Console.WriteLine("\nTesting batch lookup with auto-detection:");
        var batchResults = lookup.LookupBatch(autoTestCodes);
        foreach (var kvp in batchResults)
        {
            if (kvp.Value.HasValue)
            {
                Console.WriteLine($"  {kvp.Key} -> {kvp.Value.Value.PlaceName}");
            }
            else
            {
                Console.WriteLine($"  {kvp.Key} -> Not found");
            }
        }
        
        // Since we don't have real data yet, this will show the structure
        var result = lookup.Lookup("90210", "US");
        if (result.HasValue)
        {
            Console.WriteLine($"  Traditional lookup: Found: {result.Value.PlaceName} at {result.Value.Latitude}, {result.Value.Longitude}");
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
    Console.WriteLine("\nTo create sample data for testing, run:");
    Console.WriteLine("  dotnet run create-sample-data");
}

static void CreateSampleData()
{
    Console.WriteLine("\nCreating sample data...");
    
    var indexPath = Path.Combine("../GeoLookup.Postal/Data", "PostalCodeIndex.dat");
    var dataPath = Path.Combine("../GeoLookup.Postal/Data", "PostalCodeData.dat");
    
    try
    {
        GeoLookup.Postal.Tools.SampleDataGenerator.CreateSampleData(indexPath, dataPath);
        Console.WriteLine("✓ Sample data created successfully!");
        Console.WriteLine($"  Index file: {indexPath}");
        Console.WriteLine($"  Data file: {dataPath}");
        Console.WriteLine("\nSample postal codes included:");
        Console.WriteLine("  90210 - Beverly Hills, CA, US");
        Console.WriteLine("  10001 - New York, NY, US");
        Console.WriteLine("  K1A0B1 - Ottawa, ON, CA");
        Console.WriteLine("  M5V3L9 - Toronto, ON, CA");
        Console.WriteLine("\nYou can now test the lookup functionality!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error creating sample data: {ex.Message}");
    }
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
