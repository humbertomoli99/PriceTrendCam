using SQLite;
using SQLiteNetExtensions.Attributes;

namespace PriceTrendCam.Core.Models;
public class StoreUrl
{
    [PrimaryKey, AutoIncrement]
    public int Id
    {
        get; set;
    }

    public string Url
    {
        get; set;
    }

    [ForeignKey(typeof(Store))]
    public int StoreId
    {
        get; set;
    }

    [ManyToOne]
    public Store Store
    {
        get; set;
    }
}
