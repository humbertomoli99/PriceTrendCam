using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    public string Name
    {
        get; set;
    }
    public string Type
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
