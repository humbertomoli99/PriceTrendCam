using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PriceTrendCam.Core.Services;

namespace PriceTrendCam.Tests.MSTest;
[TestClass]
public class HtmlDocumentTests
{
    private HtmlDocument _htmlDocument;

    [TestInitialize]
    public void Initialize()
    {
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
        await HtmlDocumentService.DownloadFaviconAsync(url);
        Assert.IsTrue(File.Exists("favicon.ico"));
        File.Delete("favicon.ico");
    }

    [TestMethod]
    public async Task TestDownloadFaviconAsyncNoFaviconFound()
    {
        var url = "https://www.example.com";
        await HtmlDocumentService.DownloadFaviconAsync(url);
        Assert.IsFalse(File.Exists("favicon.ico"));
    }

    [TestMethod]
    public async Task TestDownloadFaviconAsyncInvalidUrl()
    {
        var url = "invalid url";
        await HtmlDocumentService.DownloadFaviconAsync(url);
        Assert.IsFalse(File.Exists("favicon.ico"));
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


}
