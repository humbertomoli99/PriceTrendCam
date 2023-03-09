using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using PriceTrendCam.Core.Services;
using System;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using WebDriverManager.Helpers;

namespace PriceTrendCam.Tests.MSTest;

[TestClass]
public class SeleniumTrackerServiceTests
{
    private static EdgeDriver driver;

    [ClassInitialize]
    public static void Setup(TestContext context)
    {

    }

    [TestMethod]
    public void ExecuteScript_ShouldReturnNonNullObject()
    {
        // Arrange
        string script = "document.title;";

        // Act
        string result = SeleniumTrackerService.GetGooglePageTitle();

        // Assert
        Assert.IsNotNull(result);
    }

    [ClassCleanup]
    public static void Cleanup()
    {

    }
}
