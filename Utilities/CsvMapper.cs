using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

public class CsvMapper
{
    public class DispatchRecord
    {
        public string CustomerReference { get; set; }
        public string LoadId { get; set; }
        public string ContainerType { get; set; }
        public string ItemCode { get; set; }
        public int ItemQuantity { get; set; }
        public decimal ItemWeight { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }

    public static Stream GenerateCsv(IEnumerable<DispatchRecord> records)
    {
        var stream = new MemoryStream();
        using (var writer = new StreamWriter(stream, leaveOpen: true))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(records);
        }

        stream.Position = 0; // Reset stream position
        return stream;
    }
}
