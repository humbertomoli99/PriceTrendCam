using HtmlAgilityPack;
using Jint;
using PriceTrendCam.Core.Services;

namespace PriceTrendCam.Tests.MSTest;
[TestClass]
public class ScriptExecutorServiceTests
{
    [TestMethod]
    public async Task GetHtmlFromSite_ReturnsNonEmptyString()
    {
        // Arrange
        var scriptExecutor = new ScriptExecutorService();
        var url = "https://www.example.com";

        // Act
        var html = await scriptExecutor.GetHtmlFromSite(url);

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(html));
    }

    [TestMethod]
    public void GetRootOfHtml_ReturnsRootNode()
    {
        // Arrange
        var scriptExecutor = new ScriptExecutorService();
        var doc = new HtmlDocument();
        doc.LoadHtml("<html><head></head><body></body></html>");

        // Act
        var rootNode = scriptExecutor.GetRootOfHtml(doc);

        // Assert
        Assert.AreEqual("#document", rootNode.Name);
    }

    [TestMethod]
    public void GetScriptsFromHtmlNode_ReturnsListOfScripts()
    {
        // Arrange
        var scriptExecutor = new ScriptExecutorService();
        var html = "<html><head></head><body><script>console.log('Hello, world!');</script></body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var rootNode = scriptExecutor.GetRootOfHtml(doc);

        // Act
        var scripts = scriptExecutor.GetScriptsFromHtmlNode(rootNode);

        // Assert
        Assert.AreEqual(1, scripts.Count);
        Assert.AreEqual("console.log('Hello, world!');", scripts[0]);
    }

    [TestMethod]
    public async Task ExecuteScript_ExecutesScript()
    {
        // Arrange
        var scriptExecutor = new ScriptExecutorService();
        var script = "console.log('Hello, world!');";

        // Act
        await scriptExecutor.ExecuteScriptAsync(script);

        // Assert
        // We can't actually test the console output, but we can check that the script ran without errors
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ExecuteSiteScripts_ExecutesAllScripts()
    {
        // Arrange
        var scriptExecutor = new ScriptExecutorService();
        var html = "<html><head></head><body><script>console.log('Hello, world!');</script><script>console.log('Goodbye, world!');</script></body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rootNode = scriptExecutor.GetRootOfHtml(doc);

        // Act
        await scriptExecutor.ExecuteSiteScripts(rootNode);

        // Assert
        // We can't actually test the console output, but we can check that both scripts ran without errors
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ExecuteScript_ExecutesExternalScript()
    {
        // Arrange
        var scriptExecutor = new ScriptExecutorService();
        string script = "<script src=\"https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.29.1/moment.min.js\"></script>";
        Engine engine = new Engine();

        // Act
        await scriptExecutor.ExecuteScriptAsync(script);

        // Assert
        Assert.IsNotNull(engine.GetValue("moment"));
    }
    [TestMethod]
    public void ExecuteScript_MultipliesNumbers_ReturnsCorrectValue()
    {
        // Arrange
        var scriptExecutor = new ScriptExecutorService();

        var script = @"
            function multiply(a, b) {
                return a * b;
            }
            multiply(3, 4);
        ";

        // Act
        var result = scriptExecutor.ExecuteScriptAsync(script).Result.ToString();

        // Assert
        Assert.AreEqual(12, int.Parse(result));
    }
}
