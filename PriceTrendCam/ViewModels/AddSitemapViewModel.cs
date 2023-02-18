using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Services;

namespace PriceTrendCam.ViewModels;

public partial class AddSitemapViewModel : ObservableValidator
{
    private readonly IDialogService DialogService;

    public AddSitemapViewModel(IDialogService dialogService)
    {
        SaveCommand = new AsyncRelayCommand(Save);
        TextBoxUrl = new();
        DialogService = dialogService;
    }
    public event EventHandler? FormSubmissionCompleted;
    public event EventHandler? FormSubmissionFailed;
    private Store? ObjectStore
    {
        get; set;
    }
    [ObservableProperty]
    [Required]
    private string textBoxStoreName;
    public List<string> TextBoxUrl
    {
        get; set;
    }

    public IAsyncRelayCommand SaveCommand { get; }

    private async Task Save()
    {
        ValidateAllProperties();

        if (HasErrors)
        {
            FormSubmissionFailed?.Invoke(this, EventArgs.Empty);
            return;
        }
        else
        {
            FormSubmissionCompleted?.Invoke(this, EventArgs.Empty);
        }

        var textBoxUrls = new List<StoreUrl>();

        foreach (var items in TextBoxUrl)
        {
            textBoxUrls.Add(new StoreUrl()
            {
                Url = items.ToString(),
            });
        }

        ObjectStore = new Store
        {
            Name = textBoxStoreName,
            Favicon = "favicon.png",
            Selectors = new List<Selector>(),
            Urls = textBoxUrls
        };

        await App.PriceTrackerService.InsertWithChildrenAsync<Store>(ObjectStore, true);
    }
    [RelayCommand]
    private void ShowErrors()
    {
        string message = string.Join(Environment.NewLine, GetErrors().Select(e => e.ErrorMessage));

        //_ = DialogService.ShowMessageDialogAsync("Validation errors", message);
    }

}