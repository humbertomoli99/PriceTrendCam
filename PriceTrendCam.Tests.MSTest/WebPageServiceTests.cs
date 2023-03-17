using PriceTrendCam.Core.Services;

[TestClass]
public class WebPageServiceTests
{
    private WebPageService _webPageService;

    [TestInitialize]
    public void TestInitialize()
    {
        _webPageService = new WebPageService();
    }

    [TestMethod]
    public async Task LoadPage_ShouldLoadPageContent()
    {
        // Arrange
        var url = "https://www.google.com.mx/";

        // Act
        var pageContent = await _webPageService.LoadPageAsync(url);

        // Assert
        Assert.IsNotNull(pageContent);
        Assert.IsTrue(pageContent.ChildElementCount > 0);
    }

    [TestMethod]
    public async Task SelectElement_ShouldReturnCorrectElement()
    {
        // Arrange
        var selector = "body > div.L3eUgb > div.o3j99.c93Gbe > div.uU7dJb";
        var expectedText = "México";
        var url = "https://www.google.com.mx/";

        // Act
        await _webPageService.LoadPageAsync(url);
        var selectedElement = _webPageService.SelectElement(selector);

        // Assert
        Assert.IsNotNull(selectedElement);
        Assert.AreEqual(expectedText, selectedElement.InnerHtml);
    }

    [TestMethod]
    public async Task GetAttributeValue_ShouldReturnCorrectValue()
    {
        // Arrange
        var selector = "body > div.L3eUgb > div.o3j99.c93Gbe > div.KxwPGc.SSwjIe > div.KxwPGc.AghGtd > a:nth-child(1)";
        var attributeName = "href";
        var expectedValue = "https://about.google/?utm_source=google-MX&utm_medium=referral&utm_campaign=hp-footer&fg=1";
        var url = "https://www.google.com.mx/";

        // Act
        await _webPageService.LoadPageAsync(url);
        var selectedElement = _webPageService.SelectElement(selector);
        var attributeValue = _webPageService.GetAttributeValue(selectedElement, attributeName);

        // Assert
        Assert.IsNotNull(selectedElement);
        Assert.AreEqual(expectedValue, attributeValue);
    }
}
