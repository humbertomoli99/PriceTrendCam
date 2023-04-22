﻿using SQLite;
using SQLiteNetExtensions.Attributes;

namespace PriceTrendCam.Core.Models;

public class Store
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

    public string Favicon
    {
        get; set;
    }

    public WebScrapingTools WebScrapingTool
    {
        get;set;
    }

    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<Selector> Selectors
    {
        get; set;
    }
    [OneToMany(CascadeOperations = CascadeOperation.All)]
    public List<StoreUrl> Urls
    {
        get; set;
    }
}
