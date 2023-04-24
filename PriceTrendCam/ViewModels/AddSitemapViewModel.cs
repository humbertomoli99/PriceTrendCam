using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Core.Helpers;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;
using PriceTrendCam.Helpers;

namespace PriceTrendCam.ViewModels;

public partial class AddSitemapViewModel : ObservableValidator
{
    public ContentDialogHelper<ContentDialog> ContentDialogHelper
    {
        get; set;
    }

    private readonly INavigationService navigationService;

    public AddSitemapViewModel(INavigationService navigationService)
    {
        this.navigationService = navigationService;

        ContentDialogHelper = ContentDialogHelper<ContentDialog>.Instance;
    }
    public event EventHandler? FormSubmissionCompleted;
    public event EventHandler? FormSubmissionFailed;

    private Store? ObjectStore
    {
        get; set;
    }

    [ObservableProperty]
    private string textBoxStoreName;

    [ObservableProperty]
    private string textBoxEncoding;

    [ObservableProperty]
    private string textBoxUserAgent;

    [ObservableProperty]
    private string textBoxCookie;

    [ObservableProperty]
    private string decompresionMethod;

    [ObservableProperty]
    private string selectedWebAutomationTools;

    private string message;

    public List<string> TextBoxUrls
    {
        get; set;
    }
    public XamlRoot XamlRoot
    {
        get;
        set;
    }

    [RelayCommand]
    private async Task SaveSitemap()
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

        var selectedWebBrowser = (WebScrapingTools)Enum.Parse(typeof(WebScrapingTools), SelectedWebAutomationTools);

        // Crear objeto Store y guardarlo en la base de datos
        ObjectStore = new Store
        {
            Name = TextBoxStoreName,
            Favicon = favicon,
            Selectors = new List<Selector>(),
            Urls = ListUrls,
            WebScrapingTool = selectedWebBrowser
        };
        await App.PriceTrackerService.InsertWithChildrenAsync<Store>(ObjectStore, true);

        // Limpiar valores y llamar a eventos de formulario
        ClearFormValues();
        FormSubmissionCompleted?.Invoke(this, EventArgs.Empty);

        var newstoreId = ObjectStore.Id;
        navigationService.NavigateTo(typeof(AddSelectorsViewModel).FullName!, newstoreId);
    }

    private async Task<bool> ValidateForm()
    {
        message = string.Empty;

        var tasks = TextBoxUrls.Select(async url => await Url.IsValid(url)).ToList();
        await Task.WhenAll(tasks);

        var hasInvalidUrls = tasks.Any(task => !task.Result);
        var hasEmptyName = string.IsNullOrEmpty(TextBoxStoreName);
        var hasEmptyUrls = TextBoxUrls.Any(string.IsNullOrEmpty);

        if (hasEmptyName || hasEmptyUrls || hasInvalidUrls || SelectedWebAutomationTools == null)
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
            if (SelectedWebAutomationTools == null)
            {
                message += "Select a web browser to track your product information\n";
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
    [RelayCommand]
    private async Task ShowErrors()
    {
        var dialog = new ContentDialog()
        {
            Title = "Validation errors",
            Content = message,
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        await ContentDialogHelper.ShowContentDialog(dialog);
    }
}