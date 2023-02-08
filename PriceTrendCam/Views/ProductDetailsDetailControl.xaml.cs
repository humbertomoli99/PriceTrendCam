using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.Core.Models;

namespace PriceTrendCam.Views;

public sealed partial class ProductDetailsDetailControl : UserControl
{
    public ProductInfo? ListDetailsMenuItem
    {
        get => GetValue(ListDetailsMenuItemProperty) as ProductInfo;
        set => SetValue(ListDetailsMenuItemProperty, value);
    }

    public static readonly DependencyProperty ListDetailsMenuItemProperty = DependencyProperty.Register("ListDetailsMenuItem", typeof(ProductInfo), typeof(ProductDetailsDetailControl), new PropertyMetadata(null, OnListDetailsMenuItemPropertyChanged));

    public ProductDetailsDetailControl()
    {
        InitializeComponent();
    }

    private static void OnListDetailsMenuItemPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProductDetailsDetailControl control)
        {
            control.ForegroundElement.ChangeView(0, 0, 1);
        }
    }
}
