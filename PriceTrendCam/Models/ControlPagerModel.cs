using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PriceTrendCam.Models;
public partial class ControlPagerModel : ObservableRecipient
{
    //Paginacion
    [ObservableProperty]
    public int currentPageIndex;

    [ObservableProperty]
    public int totalPagesCount;

    [ObservableProperty]
    public int totalItemsCount;

    public string PageSummary => $"Page {CurrentPageIndex + 1} of {TotalPagesCount}";

    public ObservableCollection<int> RowsPerPageOptions
    {
        get; set;
    }

    public int SelectedRowsPerPageOption
    {
        get;
        set;
    }
}
