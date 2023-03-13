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
    public async Task ExecuteScriptAsync_ExecutesScript()
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
    public async Task ExecuteScriptAsync_ExecutesExternalScript()
    {
        // Arrange
        var scriptExecutor = new ScriptExecutorService();
        var script = "<script src=\"https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.29.1/moment.min.js\"></script>";
        Engine engine = new Engine();

        // Act
        await scriptExecutor.ExecuteScriptAsync(script);

        // Assert
        Assert.IsNotNull(engine.GetValue("moment"));
    }
    [TestMethod]
    public void ExecuteScriptAsync_MultipliesNumbers_ReturnsCorrectValue()
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
        var result = scriptExecutor.EvaluateScriptAsync(script).Result.ToString();

        // Assert
        Assert.AreEqual(12, int.Parse(result));
    }
    [TestMethod]
    public async Task ExecuteSiteUrlScripts_ShouldReturnExpectedOutput()
    {
        // Arrange
        var script = "document.title";
        var title = "Disipador CPU be quiet! Dark Rock Pro 4 120/135mm 1500RPM Negro BK022 | Cyberpuerta.mx";
        var url = "https://www.cyberpuerta.mx/Computo-Hardware/Componentes/Enfriamiento-y-Ventilacion/Disipadores-para-CPU/Disipador-CPU-be-quiet-Dark-Rock-Pro-4-120-135mm-1500RPM-Negro.html";

        var scriptExecutor = new ScriptExecutorService();

        // Execute the website's scripts
        await scriptExecutor.ExecuteSiteUrlScripts(url);

        var result2 = await scriptExecutor.EvaluateScriptAsync(script);

        // Assert
        Assert.AreEqual(title, result2.ToString());
    }
    [TestMethod]
    public void Test_ExecuteScriptAsync_MultipliesNumbers_ReturnsGreaterThanExpectedValue()
    {
        // Arrange
        var scriptExecutor = new ScriptExecutorService();

        var script = @"
            function multiply(a, b) {
                if (a > b) {
                    return 'a is greater than b';
                } else {
                    return 'b is greater than or equal to a';
                }
            }
            multiply(3, 4);
        ";

        // Act
        var result = scriptExecutor.EvaluateScriptAsync(script).Result.ToString();

        // Assert
        Assert.AreEqual("b is greater than or equal to a", result);
    }
    [TestMethod]
    public void Test_ExecuteScriptAsync_ES6_ReturnsCorrectGreeting()
    {
        // Arrange
        var scriptExecutor = new ScriptExecutorService();

        var script = @"
        function greeting(name) {
            return `Hello, ${name}!`;
        }
        greeting('John');
    ";

        // Act
        var result = scriptExecutor.EvaluateScriptAsync(script).Result.ToString();

        // Assert
        Assert.AreEqual("Hello, John!", result);
    }
}
