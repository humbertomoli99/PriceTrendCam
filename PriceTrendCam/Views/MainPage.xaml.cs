using Microsoft.UI.Xaml.Controls;

using PriceTrendCam.ViewModels;

namespace PriceTrendCam.Views;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel
    {
        get;
    }

    public MainPage()
    {
        ViewModel = App.GetService<MainViewModel>();
        InitializeComponent();
    }

    private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        //obtenemos todos los url de busqueda de productos
        //hacemos condicional para averiguar si es una url valida
        //si es una url valida obtendremos su store y selectores
        //si es una url que tiene varios productos de terceros avisaremos al usuario de si quiere añadir envio masivo de urls
        // si no continuamos con el registro comun de los productos
    }
}
