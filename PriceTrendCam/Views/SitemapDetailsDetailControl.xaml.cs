using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.Core.Models;

namespace PriceTrendCam.Views;

public sealed partial class SitemapDetailsDetailControl : UserControl
{
    public Store? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as Store;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }

    public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(Store), typeof(SitemapDetailsDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

    public SitemapDetailsDetailControl()
    {
        InitializeComponent();
    }

    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SitemapDetailsDetailControl control)
        {
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
