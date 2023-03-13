using SQLite;
using SQLiteNetExtensions.Attributes;

namespace PriceTrendCam.Core.Models;
public class Selector
{
    [PrimaryKey, AutoIncrement]
    public int Id
    {
        get; set;
    }
    public string Command
    {
        get; set;
    }
    public string CssSelector
    {
        get; set;
    }
    public string Type
    {
        get; set;
    }
    public string Pattern
    {
        get; set;
    }
    public string Replacement
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
