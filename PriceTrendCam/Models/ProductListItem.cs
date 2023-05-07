using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PriceTrendCam.Models;
public partial class ProductListItem : ObservableObject
{
    [ObservableProperty]
    private int id;

    [ObservableProperty]
    private string? title;

    [ObservableProperty]
    private string? imageLocation;

    [ObservableProperty]
    private string? price;

    [ObservableProperty]
    private string? shipping;

    [ObservableProperty]
    private string? stock;

    [ObservableProperty]
    private ICommand? command;
}
