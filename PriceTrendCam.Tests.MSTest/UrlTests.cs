using PriceTrendCam.Core.Helpers;

namespace PriceTrendCam.Tests.MSTest;

[TestClass]
public class UrlTests
{
    private static string validUrl;
    private static string invalidUrl;
    private static string redirectingUrl;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        validUrl = "https://www.example.com";
        invalidUrl = "invalidurl";
        redirectingUrl = "https://www.example.com/redirect";
    }

    [TestMethod]
    public void IsValid_ValidUrl_ReturnsTrue()
    {
        // Act
        bool isValid = Url.IsValid(validUrl).Result;

        // Assert
        Assert.IsTrue(isValid, "The URL should be valid.");
    }

    [TestMethod]
    public void IsValid_InvalidUrl_ReturnsFalse()
    {
        // Act
        bool isValid = Url.IsValid(invalidUrl).Result;

        // Assert
        Assert.IsFalse(isValid, "The URL should be invalid.");
    }

    [TestMethod]
    public void GetRedirectUrl_RedirectingUrl_ReturnsFinalUrl()
    {
        // Act
        string finalUrl = Url.GetRedirectUrl(redirectingUrl).Result;

        // Assert
        Assert.AreEqual("https://www.example.com/final", finalUrl, "The final URL should be correct.");
    }

    [TestMethod]
    public void LimpiarUrl_UrlWithQueryString_ReturnsCleanedUrl()
    {
        // Arrange
        string urlWithQuery = "https://www.example.com/product?id=123";

        // Act
        string cleanedUrl = Url.LimpiarUrl(urlWithQuery);

        // Assert
        Assert.AreEqual("https://www.example.com/product", cleanedUrl, "The cleaned URL should not include the query string.");
    }
}