public class LoadData
{
    public int ControlNumber { get; set; }
    public string SalesOrder { get; set; }
    public DeliveryAddress DeliveryAddress { get; set; }
    public List<Container> Containers { get; set; }
}

public class DeliveryAddress
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
}

public class Container
{
    public string LoadId { get; set; }
    public string ContainerType { get; set; }
    public List<Item> Items { get; set; }
}

public class Item
{
    public string ItemCode { get; set; }
    public int Quantity { get; set; }
    public decimal CartonWeight { get; set; }
}
