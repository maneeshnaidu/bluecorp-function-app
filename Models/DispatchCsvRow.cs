using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bluecorp_function_app.Models
{
    public class DispatchCsvRow
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

}