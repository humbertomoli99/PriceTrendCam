using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceTrendCam.Core.Models;
public class SearchActionData
{
    public string Name
    {
        get; set;
    }
    public string WebsiteUrl
    {
        get; set;
    }
    public string SearchUrl
    {
        get; set;
    }
    public string QueryParam
    {
        get; set;
    }
}