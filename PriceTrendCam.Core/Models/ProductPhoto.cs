using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace PriceTrendCam.Core.Models;
public class ProductPhoto
{
    [PrimaryKey, AutoIncrement]
    public int Id
    {
        get; set;
    }

    public string Src
    {
        get; set;
    }

    [ForeignKey(typeof(ProductInfo))]
    public int ProductId
    {
        get; set;
    }
}
