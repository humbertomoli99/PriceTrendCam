using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace PriceTrendCam.Core.Models;
public class Notification
{
    [PrimaryKey, AutoIncrement]
    public int Id
    {
        get; set;
    }

    [ForeignKey(typeof(ProductInfo))]
    public int ProductId
    {
        get; set;
    }

    public int Title
    {
        get; set;
    }

    public string Message
    {
        get; set;
    }

    public double NewPrice
    {
        get; set;
    }

    public double PreviousPrice
    {
        get; set;
    }
}
