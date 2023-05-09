// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PriceTrendCam.ViewModels;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PriceTrendCam.Controls;
public sealed partial class ControlPager : UserControl
{
    public ControlPagerViewModel ViewModel
    {
        get;
    }
    public ControlPager()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<ControlPagerViewModel>();
    }

    private async void RowsPerPageOptions_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            if (RowsPerPageOptions.SelectedItem != null && int.TryParse(RowsPerPageOptions.SelectedItem as string, out int selectedValue))
            {
                viewModel.SelectedRowsPerPageOption = selectedValue;
                await ViewModel.OnSelectedItemChanged();
            }
        }
    }
}
