using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;
#nullable enable

namespace PriceTrendCam.Core.Models;
public class ProductInfo
{
    [PrimaryKey, AutoIncrement]
    public int Id
    {
        get; set;
    }

    public string? Name
    {
        get; set;
    }
    public string? Description
    {
        get; set;
    }
    public string? Url
    {
        get; set;
    }
    public double? Price
    {
        get; set;
    }
    public string? PriceCurrency
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
    public string? ShippingCurrency
    {
        get; set;
    }
    public string? StoreName
    {
        get; set;
    }
    public ProductStatus Status
    {
        get;
        set;
    }
    public double? Stock
    {
        get; set;
    }
    public string? Image
    {
        get; set;
    }

    [ForeignKey(typeof(Store))]
    public int StoreId
    {
        get; set;
    }

    [OneToOne]
    public Store? Store
    {
        get; set;
    }
    [OneToOne]

    public Store? Favicon
    {
        get; set;
    }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<History>? Histories
    {
        get; set;
    }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<ProductPhoto>? Photos
    {
        get; set;
    }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<Notification>? Notifications
    {
        get; set;
    }
}
