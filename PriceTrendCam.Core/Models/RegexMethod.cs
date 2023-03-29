using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace PriceTrendCam.Core.Models;
public class RegexMethod
{
    [PrimaryKey, AutoIncrement]
    public int Id
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

    [ForeignKey(typeof(Selector))]
    public int SelectorId
    {
        get; set;
    }

    [ManyToOne]
    public Selector Selector
    {
        get; set;
    }
}
