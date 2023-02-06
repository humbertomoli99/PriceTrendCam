using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace PriceTrendCam.Core.Models;
public class History
{
    [PrimaryKey, AutoIncrement]
    public int Id
    {
        get; set;
    }

    [ForeignKey(typeof(ProductInfo))]
    public int ProductInfoId
    {
        get; set;
    }

    public double? Price
    {
        get; set;
    }
    public int? PriceChange
    {
        get; set;
    }
    public DateTime Date
    {
        get; set;
    }
    public double? ShippingPrice
    {
        get; set;
    }
    public int? Stock
    {
        get; set;
    }

    [OneToOne]
    public ProductInfo ProductInfo
    {
        get; set;
    }
}
