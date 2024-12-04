using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bluecorp_function_app.Utilities
{
    public static class CsvMapper
    {
        public static string MapToCsv(LoadData data)
    {
        var csvLines = new List<string>
        {
            "CustomerReference,LoadId,ContainerType,ItemCode,ItemQuantity,ItemWeight,Street,City,State,PostalCode,Country"
        };

        foreach (var container in data.Containers)
        {
            foreach (var item in container.Items)
            {
                csvLines.Add($"{data.SalesOrder},{container.LoadId},{ConvertContainerType(container.ContainerType)}," +
                             $"{item.ItemCode},{item.Quantity},{item.CartonWeight}," +
                             $"{data.DeliveryAddress.Street},{data.DeliveryAddress.City},{data.DeliveryAddress.State}," +
                             $"{data.DeliveryAddress.PostalCode},{data.DeliveryAddress.Country}");
            }
        }

        return string.Join(Environment.NewLine, csvLines);
    }

    private static string ConvertContainerType(string containerType) =>
        containerType switch
        {
            "20RF" => "REF20",
            "40RF" => "REF40",
            "20HC" => "HC20",
            "40HC" => "HC40",
            _ => containerType
        };   
    }
}