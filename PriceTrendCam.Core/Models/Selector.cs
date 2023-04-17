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
    public bool NotNull
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
    public string Attribute
    {
        get; set;
    }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<RegexMethod> RegexMethods
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
