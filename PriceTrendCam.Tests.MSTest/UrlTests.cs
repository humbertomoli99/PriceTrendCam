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
        redirectingUrl = "http://articulo.mercadolibre.com.mx/";
    }

    [TestMethod]
    public void IsValid_ValidUrl_ReturnsTrue()
    {
        // Act
        var isValid = Url.IsValid(validUrl).Result;

        // Assert
        Assert.IsTrue(isValid, "The URL should be valid.");
    }

    [TestMethod]
    public void IsValid_InvalidUrl_ReturnsFalse()
    {
        // Act
        var isValid = Url.IsValid(invalidUrl).Result;

        // Assert
        Assert.IsFalse(isValid, "The URL should be invalid.");
    }

    [TestMethod]
    public void GetRedirectUrl_RedirectingUrl_ReturnsFinalUrl()
    {
        // Act
        var finalUrl = Url.GetRedirectUrl(redirectingUrl).Result;

        // Assert
        Assert.AreEqual("https://www.mercadolibre.com.mx/", finalUrl, "The final URL should be correct.");
    }

    [TestMethod]
    public void LimpiarUrl_UrlWithQueryString_ReturnsCleanedUrl()
    {
        // Arrange
        var urlWithQuery = "https://www.example.com/product?id=123";

        // Act
        var cleanedUrl = Url.LimpiarUrl(urlWithQuery);

        // Assert
        Assert.AreEqual("https://www.example.com/product", cleanedUrl, "The cleaned URL should not include the query string.");
    }

    [TestMethod]
    public void LimpiarUrl_UrlWithQueryString_ReturnsCleanedUrl2()
    {
        // Arrange
        var urlWithQuery = "https://www.mercadolibre.com.mx/memoria-ram-fury-impact-gamer-color-negro-16gb-1-kingston-kf432s20ib16/p/MLM18931517?#reco_item_pos=1&reco_backend=machinalis-homes-pdp-boos&reco_backend_type=function&reco_client=home_navigation-recommendations&reco_id=9682a945-44b4-4232-8966-11f96f08b4e4&c_id=/home/navigation-recommendations/element&c_element_order=2&c_uid=3ec356be-c5a6-4fea-b3e8-083f62abbab2";

        // Act
        var cleanedUrl = Url.LimpiarUrl(urlWithQuery);

        // Assert
        Assert.AreEqual("https://www.mercadolibre.com.mx/memoria-ram-fury-impact-gamer-color-negro-16gb-1-kingston-kf432s20ib16/p/MLM18931517", cleanedUrl, "The cleaned URL should not include the query string.");
    }
}