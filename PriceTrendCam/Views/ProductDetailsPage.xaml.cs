﻿using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class ProductDetailsPage : Page
{
    public ProductDetailsViewModel ViewModel
    {
        get;
    }

    public ProductDetailsPage()
    {
        ViewModel = App.GetService<ProductDetailsViewModel>();
        InitializeComponent();
        Loaded += ProductDetailsPage_Loaded;
    }

    private void ProductDetailsPage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Establecer el contexto de datos del ListView
        DataContext = ViewModel;
    }

    private void flip_SizeChanged(object sender, Microsoft.UI.Xaml.SizeChangedEventArgs e)
    {
        this.flip.Height = this.ActualWidth * 0.5625;
    }
}
