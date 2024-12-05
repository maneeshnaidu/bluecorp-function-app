using System.Collections.Generic;
using System.Globalization;
using System.IO;
using bluecorp_function_app.Interfaces;
using bluecorp_function_app.Models;
using CsvHelper;

public class JsonToCsvMapper : IJsonToCsvMapper
{
    public void MapToCsv(LoadData loadData, string outputPath)
    {
        // var stream = new MemoryStream();
        using var writer = new StreamWriter(outputPath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteHeader<DispatchCsvRow>();
        csv.NextRecord();

        foreach (var container in loadData.Containers)
        {
            foreach (var item in container.Items)
            {
                csv.WriteRecord(new DispatchCsvRow
                {
                    CustomerReference = loadData.SalesOrder,
                    LoadId = container.LoadId,
                    ContainerType = ConvertContainerType(container.ContainerType),
                    ItemCode = item.ItemCode,
                    ItemQuantity = item.Quantity,
                    ItemWeight = item.CartonWeight,
                    Street = loadData.DeliveryAddress.Street,
                    City = loadData.DeliveryAddress.City,
                    State = loadData.DeliveryAddress.State,
                    PostalCode = loadData.DeliveryAddress.PostalCode,
                    Country = loadData.DeliveryAddress.Country
                });
                csv.NextRecord();
            }
        }

        // stream.Position = 0; // Reset stream position
        // return stream;
    }

    private static string ConvertContainerType(string containerType)
    {
        return containerType switch
        {
            "20RF" => "REF20",
            "40RF" => "REF40",
            "20HC" => "HC20",
            "40HC" => "HC40",
            _ => containerType
        };
    }
}
