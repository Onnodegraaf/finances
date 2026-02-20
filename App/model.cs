using System.Globalization;
using System.IO;
using System.Text.Json.Serialization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

public record Transaction
{
    [JsonIgnore]
    public string ID => CreateMD5($"{Date}{Description}{Out}{In}");
    [Index(0)]
    public DateOnly Date { get; init; }
    
    [Index(1)]    
    public string Description { get; init; }

    [Index(2)]
    public decimal? Out {get; init; }
    [Index(3)]
    public decimal? In {get; init; }

    [Index(4)]
    public string? Label{get; init; }
    [Index(5)]
    public string? SubLabel{get; init; }

    

    public static Transaction[] ReadCsvRecords(StreamReader reader)
{
    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false, // Crucial for no headers
            MissingFieldFound = null
        };
    // Use 'using' statements to ensure the StreamReader and CsvReader are properly disposed
   using (var csv = new CsvReader(reader, configuration:config))
    {
        // Get all records from the CSV file and convert them to an array
        var records = csv.GetRecords<Transaction>().ToArray();
        return records;
    }
}

static string CreateMD5(ReadOnlySpan<char> input)
{
    var encoding = System.Text.Encoding.UTF8;
    var inputByteCount = encoding.GetByteCount(input);
    using var md5 = System.Security.Cryptography.MD5.Create();

    Span<byte> bytes = inputByteCount < 1024
        ? stackalloc byte[inputByteCount]
        : new byte[inputByteCount];
    Span<byte> destination = stackalloc byte[md5.HashSize / 8];

    encoding.GetBytes(input, bytes);

    // checking the result is not required because this only returns false if "(destination.Length < HashSizeValue/8)", which is never true in this case
    md5.TryComputeHash(bytes, destination, out int _bytesWritten);

    return BitConverter.ToString(destination.ToArray());
}
}
/*
2026-02-02,Electronic Funds Transfer PREAUTHORIZED DEBIT WPG-TIPP Feb/26,342.00,
&*/

