using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace PriceTrendCam.Core.Services;
public class SeleniumTrackerService
{
    public static string GetGooglePageTitle()
    {
        using var driverService = EdgeDriverService.CreateDefaultService();
        driverService.EnableVerboseLogging = false;
        driverService.HideCommandPromptWindow = true;

        using var driver = new EdgeDriver(driverService, GetEdgeOptions());
        driver.Navigate().GoToUrl("https://www.google.com/");

        var title = driver.Title;

        return title;
    }

    private static EdgeOptions GetEdgeOptions()
    {
        var options = new EdgeOptions();
        options.AddArgument("headless");
        options.AddArgument("no-sandbox");
        options.AddArgument("--remote-debugging-port=9245");
        options.PageLoadStrategy = PageLoadStrategy.Normal;
        options.AddUserProfilePreference("profile.cache.size", 1024 * 1024 * 1024);
        options.AddUserProfilePreference("profile.default_content_setting_values.user_agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299");
        options.AddUserProfilePreference("profile.default_content_setting_values.cookies", 1);
        options.AddUserProfilePreference("profile.default_content_setting_values.cache", 1);

        return options;
    }
}