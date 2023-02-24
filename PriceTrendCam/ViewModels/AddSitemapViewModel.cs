using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;

namespace PriceTrendCam.ViewModels;

public partial class AddSitemapViewModel : ObservableValidator
{
    private readonly IDialogService DialogService;

    public AddSitemapViewModel(IDialogService dialogService)
    {
        TextBoxUrls = new();
        DialogService = dialogService;

        //metodos asincronos mvvm community
        SaveSitemapCommand = new AsyncRelayCommand(SaveSitemapAsync);
        //ShowErrorsCommand = new AsyncRelayCommand(ShowErrorsAsync);
    }

    public IAsyncRelayCommand SaveSitemapCommand
    {
        get;
    }
    //public IAsyncRelayCommand ShowErrorsCommand
    //{
    //    get;
    //}
    public event EventHandler? FormSubmissionCompleted;
    public event EventHandler? FormSubmissionFailed;

    private Store? ObjectStore
    {
        get; set;
    }

    [ObservableProperty]
    private string textBoxStoreName;
    private string message;

    public List<string> TextBoxUrls
    {
        get; set;
    }

    private async Task SaveSitemapAsync()
    {
        // Validar entrada
        if ( await ValidateForm() == false )
        {
            FormSubmissionFailed?.Invoke(this, EventArgs.Empty);
            return;
        }

        // Crear lista de StoreUrl
        var ListUrls = TextBoxUrls
            .Select(url => new StoreUrl { Url = url.ToString() })
            .ToList();

        // Obtener favicon de la primera URL
        var FirstUrl = ListUrls.First().Url;
        var favicon = await HtmlDocumentService.GetFaviconUrlAsync(FirstUrl);

        // Crear objeto Store y guardarlo en la base de datos
        ObjectStore = new Store
        {
            Name = textBoxStoreName,
            Favicon = favicon,
            Selectors = new List<Selector>(),
            Urls = ListUrls
        };
        await App.PriceTrackerService.InsertWithChildrenAsync<Store>(ObjectStore, true);
        
        // Limpiar valores y llamar a eventos de formulario
        ClearFormValues();
        FormSubmissionCompleted?.Invoke(this, EventArgs.Empty);

        var newstoreId = ObjectStore.Id;
        var navigationService = App.GetService<INavigationService>();
        navigationService.NavigateTo(typeof(AddSelectorsViewModel).FullName!, newstoreId);
    }

    private async Task<bool> ValidateForm()
    {
        message = string.Empty;
        
        var tasks = TextBoxUrls.Select(async url => await Url.IsValid(url)).ToList();
        await Task.WhenAll(tasks);

        var hasInvalidUrls = tasks.Any(task => !task.Result);
        var hasEmptyName = string.IsNullOrEmpty(textBoxStoreName);
        var hasEmptyUrls = TextBoxUrls.Any(string.IsNullOrEmpty);

        if (hasEmptyName || hasEmptyUrls || hasInvalidUrls)
        {
            if (hasEmptyName)
            {
                message += "The text field is required.\n";
            }
            if (hasEmptyUrls)
            {
                message += "Not all textboxes have urls\n";
            }
            else if (hasInvalidUrls)
            {
                message += "Invalid Url\n";
            }
            ClearFormValues();
            return false;
        }

        return true;
    }

    private void ClearFormValues()
    {
        TextBoxUrls.Clear();
    }
    public async Task ShowErrorsAsync(XamlRoot xamlRoot)
    {
        await DialogService.ShowMessageDialogAsync("Validation errors", message, xamlRoot);
    }
}