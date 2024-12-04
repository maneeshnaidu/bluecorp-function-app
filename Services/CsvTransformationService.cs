using System.Text;

public static class CsvTransformationService
{
    public static string Transform(LoadData payload)
    {
        var csv = new StringBuilder();
        csv.AppendLine("CustomerReference,LoadId,ContainerType,ItemCode,ItemQuantity,ItemWeight,Street,City,State,PostalCode,Country");

        foreach (var container in payload.Containers)
        {
            foreach (var item in container.Items)
            {
                csv.AppendLine($"{payload.SalesOrder},{container.LoadId},{ConvertContainerType(container.ContainerType)},{item.ItemCode},{item.Quantity},{item.CartonWeight},{payload.DeliveryAddress.Street},{payload.DeliveryAddress.City},{payload.DeliveryAddress.State},{payload.DeliveryAddress.PostalCode},{payload.DeliveryAddress.Country}");
            }
        }

        return csv.ToString();
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
