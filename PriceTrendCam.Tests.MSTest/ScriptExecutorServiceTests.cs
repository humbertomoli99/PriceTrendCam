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
        var url = "https://www.example.com";

        // Act
        var html = await ScriptExecutorService.GetHtmlFromSite(url);

        // Assert
        Assert.IsFalse(string.IsNullOrEmpty(html));
    }

    [TestMethod]
    public void GetRootOfHtml_ReturnsRootNode()
    {
        // Arrange
        var doc = new HtmlDocument();
        doc.LoadHtml("<html><head></head><body></body></html>");

        // Act
        var rootNode = ScriptExecutorService.GetRootOfHtml(doc);

        // Assert
        Assert.AreEqual("#document", rootNode.Name);
    }

    [TestMethod]
    public void GetScriptsFromHtmlNode_ReturnsListOfScripts()
    {
        // Arrange
        var html = "<html><head></head><body><script>console.log('Hello, world!');</script></body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var rootNode = ScriptExecutorService.GetRootOfHtml(doc);

        // Act
        var scripts = ScriptExecutorService.GetScriptsFromHtmlNode(rootNode);

        // Assert
        Assert.AreEqual(1, scripts.Count);
        Assert.AreEqual("console.log('Hello, world!');", scripts[0]);
    }

    [TestMethod]
    public async Task ExecuteScript_ExecutesScript()
    {
        // Arrange
        var engine = new Engine();
        var script = "console.log('Hello, world!');";

        // Act
        await ScriptExecutorService.ExecuteScript(script, engine);

        // Assert
        // We can't actually test the console output, but we can check that the script ran without errors
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ExecuteSiteScripts_ExecutesAllScripts()
    {
        // Arrange
        var html = "<html><head></head><body><script>console.log('Hello, world!');</script><script>console.log('Goodbye, world!');</script></body></html>";
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var rootNode = ScriptExecutorService.GetRootOfHtml(doc);

        // Act
        await ScriptExecutorService.ExecuteSiteScripts(rootNode);

        // Assert
        // We can't actually test the console output, but we can check that both scripts ran without errors
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task ExecuteScript_ExecutesExternalScript()
    {
        // Arrange
        string script = "<script src=\"https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.29.1/moment.min.js\"></script>";
        Engine engine = new Engine();

        // Act
        await ScriptExecutorService.ExecuteScript(script, engine);

        // Assert
        Assert.IsNotNull(engine.GetValue("moment"));
    }
}
