using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using PriceTrendCam.Activation;
using PriceTrendCam.Contracts.Services;
using PriceTrendCam.Core.Contracts.Services;
using PriceTrendCam.Core.Models;
using PriceTrendCam.Core.Services;
using PriceTrendCam.Helpers;
using PriceTrendCam.Models;
using PriceTrendCam.Notifications;
using PriceTrendCam.Services;
using PriceTrendCam.Services.DataService;
using PriceTrendCam.ViewModels;
using PriceTrendCam.Views;

namespace PriceTrendCam;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
    // The .NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging

    private static readonly PriceTrackerDbService _productService =
    new(Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "Product.db3"));

    public static PriceTrackerDbService PriceTrackerService => _productService;
    public IHost Host
    {
        get;
    }

    public static T GetService<T>()
        where T : class
    {
        if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
        {
            throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
        }

        return service;
    }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public App()
    {
        InitializeComponent();

        AppCenter.Start("b6fbd58c-f2e8-447d-8b8e-fd51717ec232",
                  typeof(Analytics), typeof(Crashes));

        Host = Microsoft.Extensions.Hosting.Host.
        CreateDefaultBuilder().
        UseContentRoot(AppContext.BaseDirectory).
        ConfigureServices((context, services) =>
        {
            // Default Activation Handler
            services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

            // Other Activation Handlers
            services.AddTransient<IActivationHandler, AppNotificationActivationHandler>();

            // Services
            services.AddSingleton<IAppNotificationService, AppNotificationService>();
            services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
            services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
            services.AddTransient<IWebViewService, WebViewService>();
            services.AddTransient<INavigationViewService, NavigationViewService>();
            services.AddTransient<IClipboardSelectorService, ClipboardSelectorService>();

            services.AddSingleton<IActivationService, ActivationService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<INavigationService, NavigationService>();

            // Core Services
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<ISampleDataService<SampleOrder>, SampleDataService>();
            services.AddSingleton<ISampleDataService<ProductInfo>, ProductInfoDataService>();
            services.AddSingleton<ISampleDataService<Store>, StoreDataService>();
            services.AddSingleton<IFileService, FileService>();

            // Views and ViewModels
            services.AddTransient<ProductDetailsViewModel>();
            services.AddTransient<ProductDetailsPage>();
            services.AddTransient<PostManFormViewModel>();
            services.AddTransient<PostManFormPage>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SettingsPage>();
            services.AddTransient<PackageTrackerViewModel>();
            services.AddTransient<PackageTrackerPage>();
            services.AddTransient<NotificationsViewModel>();
            services.AddTransient<NotificationsPage>();
            services.AddTransient<ContentGridDetailViewModel>();
            services.AddTransient<ContentGridDetailPage>();
            services.AddTransient<ContentGridViewModel>();
            services.AddTransient<ContentGridPage>();
            services.AddTransient<ExportDataViewModel>();
            services.AddTransient<ExportDataPage>();
            services.AddTransient<AddSelectorsViewModel>();
            services.AddTransient<AddSelectorsPage>();
            services.AddTransient<SelectorListViewModel>();
            services.AddTransient<SelectorListPage>();
            services.AddTransient<AddSitemapViewModel>();
            services.AddTransient<AddSitemapPage>();
            services.AddTransient<SitemapListViewModel>();
            services.AddTransient<SitemapListPage>();
            services.AddTransient<ProductDetailsVCViewModel>();
            services.AddTransient<ProductDetailsVCPage>();
            services.AddTransient<ProductsListViewModel>();
            services.AddTransient<ProductsListPage>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<ShellPage>();
            services.AddTransient<ShellViewModel>();

            // Configuration
            services.Configure<LocalSettingsOptions>(context.Configuration.GetSection(nameof(LocalSettingsOptions)));
        }).
        Build();

        App.GetService<IAppNotificationService>().Initialize();

        UnhandledException += App_UnhandledException;
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // TODO: Log and handle exceptions as appropriate.
        // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        base.OnLaunched(args);

        App.GetService<IAppNotificationService>().Show(string.Format("AppNotificationSamplePayload".GetLocalized(), AppContext.BaseDirectory));

        await App.GetService<IActivationService>().ActivateAsync(args);
    }
}
