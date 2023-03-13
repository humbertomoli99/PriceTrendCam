using HtmlAgilityPack;
using PriceTrendCam.Core.Services;

namespace PriceTrendCam.Tests.MSTest;
[TestClass]
public class HtmlDocumentTests
{
    private HtmlDocument _htmlDocument;
    private string pathFolder;
    [TestInitialize]
    public void Initialize()
    {
        pathFolder = $"C:/Users/humbe/AppData/Local/Packages/5F0BDD47-3323-4D23-87E2-10643AB2F138_penprprrxct9c/LocalState/";
        var html = @"<html>
                        <head>
                            <title>Test Title</title>
                            <meta name='description' content='Test Description'>
                            <meta property='og:image' content='https://www.example.com/image.jpg'>
                        </head>
                        <body></body>
                    </html>";

        _htmlDocument = new HtmlDocument();
        _htmlDocument.LoadHtml(html);
    }
    [TestMethod]
    public void GetDocumentNode_ReturnsCorrectValue()
    {
        // Arrange
        var expected = "Test Title";

        // Act
        var node = HtmlDocumentService.LoadPageAsync("https://www.google.com.mx/");

        // Assert
        Assert.IsNotNull(node);
    }
    [TestMethod]
    public void GetMetaTitle_ReturnsCorrectValue()
    {
        // Arrange
        var expected = "Test Title";

        // Act
        var actual = HtmlDocumentService.GetMetaTitle(_htmlDocument.DocumentNode);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void GetMetaDescription_ReturnsCorrectValue()
    {
        // Arrange
        var expected = "Test Description";

        // Act
        var actual = HtmlDocumentService.GetMetaDescription(_htmlDocument.DocumentNode);

        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void GetMetaImage_ReturnsCorrectValue()
    {
        // Arrange
        var expected = "https://www.example.com/image.jpg";

        // Act
        var actual = HtmlDocumentService.GetMetaImage(_htmlDocument.DocumentNode);

        // Assert
        Assert.AreEqual(expected, actual);
    }
    [TestMethod]
    public async Task TestDownloadFaviconAsyncSuccess()
    {
        var url = "https://www.google.com";
        var filePath = Path.Combine(pathFolder, "google_com_favicon.ico");

        await HtmlDocumentService.DownloadFaviconAsync(url, pathFolder);

        Assert.IsTrue(File.Exists(filePath));
        File.Delete(filePath);
    }
    public async Task TestDownloadFaviconAsyncSuccess1()
    {
        var url = "https://www.mercadolibre.com.mx/";
        var filePath = Path.Combine(pathFolder, "mercadolibre_com_mx.ico");

        await HtmlDocumentService.DownloadFaviconAsync(url, pathFolder);

        Assert.IsTrue(File.Exists(filePath));
        File.Delete(filePath);
    }
    public async Task TestDownloadFaviconAsyncSuccess2()
    {
        var url = "https://www.cyberpuerta.mx/";
        var filePath = Path.Combine(pathFolder, "cyberpuerta_mx.ico");

        await HtmlDocumentService.DownloadFaviconAsync(url, pathFolder);

        Assert.IsTrue(File.Exists(filePath));
        File.Delete(filePath);
    }
    [TestMethod]
    public async Task TestDownloadFaviconAsyncNoFaviconFound()
    {
        var url = "https://www.example.com";
        var filePath = Path.Combine(pathFolder, "google_com_favicon.ico");

        await HtmlDocumentService.DownloadFaviconAsync(url, pathFolder);
        Assert.IsFalse(File.Exists(filePath));
    }

    [TestMethod]
    public async Task TestDownloadFaviconAsyncInvalidUrl()
    {
        var url = "invalid url";
        await HtmlDocumentService.DownloadFaviconAsync(url, pathFolder);
        Assert.IsFalse(File.Exists(Path.Combine(pathFolder, "google_com_favicon.ico")));
    }
    [TestClass]
    public class LoadPageAsyncTests
    {
        [TestMethod]
        public async Task TestLoadPageAsync()
        {
            // Arrange
            var requestUri = "https://www.example.com";

            // Act
            var result = await HtmlDocumentService.LoadPageAsync(requestUri);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(HtmlNode));
        }
    }
    [TestClass]
    public class GetUrlsTests
    {
        [TestMethod]
        public void TestGetUrls()
        {
            // Arrange
            var html = "<html><body><a href='https://www.example.com'>Example Link</a></body></html>";
            var document = new HtmlDocument();
            document.LoadHtml(html);
            var htmlUrl = document.DocumentNode;

            // Act
            var result = HtmlDocumentService.GetUrls(htmlUrl);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<string>));
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("https://www.example.com", result[0]);
        }
    }

    [TestClass]
    public class RemoveDuplicateUrlsTests
    {
        [TestMethod]
        public async Task TestRemoveDuplicateUrls_ValidInput()
        {
            // Arrange
            var url = "https://listado.mercadolibre.com.mx/gtx-1060-6gb#D[A:gtx%201060%206gb,L:undefined]";

            // Act
            var result = await HtmlDocumentService.RemoveDuplicateUrls(url);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            CollectionAssert.AllItemsAreUnique(result);
        }
    }
    [TestClass]
    public class SearchActionTests
    {
        [TestMethod]
        public async Task TestParseWebSiteJsonLdForSearchAction()
        {
            // Arrange
            var html = @"
                <html>
                    <head>
                        <meta property=""og:site_name"" content=""Test Site"" />
                        <meta property=""og:url"" content=""https://www.testsite.com"" />
                    </head>
                    <body>
                        <script type=""application/ld+json"">
                            {
                                ""@context"": ""https://schema.org"",
                                ""@type"": ""WebSite"",
                                ""potentialAction"": {
                                    ""@type"": ""SearchAction"",
                                    ""target"": ""https://www.testsite.com/search?q={search_term_string}"",
                                    ""query-input"": ""required name=search_term_string""
                                }
                            }
                        </script>
                    </body>
                </html>";
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Act
            var searchActionData = await HtmlDocumentService.ParseWebSiteJsonLdForSearchAction(doc.DocumentNode);

            // Assert
            Assert.AreEqual("Test Site", searchActionData.Name);
            Assert.AreEqual("https://www.testsite.com", searchActionData.WebsiteUrl);
            Assert.AreEqual("https://www.testsite.com/search?q={search_term_string}", searchActionData.SearchUrl);
            Assert.AreEqual("required name=search_term_string", searchActionData.QueryParam);
        }
    }
}
