using System.Diagnostics;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Jint;

namespace PriceTrendCam.Core.Services;
public class ScriptExecutorService
{
    private static Engine _engine = new Engine();

    public ScriptExecutorService()
    {
        _engine = new Engine();
    }
    public async Task ExecuteSiteUrlScripts(string url)
    {
        // Get the website's HTML
        var html = await GetHtmlFromSite(url);

        // Load the HTML into a HtmlDocument
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Get the root node of the HTML document
        var rootNode = GetRootOfHtml(doc);

        await ExecuteSiteScripts(rootNode);
    }
    public async Task ExecuteSiteScripts(HtmlNode siteNode)
    {
        // Get all scripts from the site
        var scripts = GetScriptsFromHtmlNode(siteNode);

        // Execute each script in order
        foreach (var script in scripts)
        {
            try
            {
                await ExecuteScriptAsync(script);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception occurred while executing script: {ex.Message}");
            }
        }
    }
    public async Task<string> GetHtmlFromSite(string url)
    {
        // Create an HTTP client and get the website's HTML
        using (var httpClient = new System.Net.Http.HttpClient())
        {
            var response = await httpClient.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
    }

    public HtmlNode GetRootOfHtml(HtmlDocument doc)
    {
        return doc.DocumentNode;
    }

    public List<string> GetScriptsFromHtmlNode(HtmlNode node)
    {
        var scripts = new List<string>();
        // Buscar nodos de script y agregar el contenido de cada nodo a la lista de scripts
        foreach (var scriptNode in node.Descendants("script"))
        {
            if (!string.IsNullOrEmpty(scriptNode.InnerText))
            {
                scripts.Add(scriptNode.InnerText.Trim());
            }
        }

        return scripts;
    }

    public async Task<object> EvaluateScriptAsync(string script)
    {
        try
        {
            // Provide a custom implementation of the console object to Jint
            _engine.SetValue("console", new CustomConsole());

            if (script.StartsWith("<script") && script.Contains("src="))
            {
                // The script is loaded from an external URL
                var startIndex = script.IndexOf("src=") + 5;
                var endIndex = script.IndexOf("\"", startIndex);
                var url = script.Substring(startIndex, endIndex - startIndex);

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(url);
                    var content = await response.Content.ReadAsStringAsync();
                    return _engine.Execute(content).GetCompletionValue();
                }
            }
            else
            {
                // The script is contained within the HTML page
                script = script.Replace("<script>", "").Replace("</script>", "");
                return _engine.Execute(script).GetCompletionValue();
            }
        }
        catch (Jint.Parser.ParserException ex)
        {
            Debug.WriteLine($"Jint ParserException occurred: {ex.Message}");
            throw;
        }
    }
    public async Task ExecuteScriptAsync(string script)
    {
        try
        {
            // Provide a custom implementation of the console object to Jint
            _engine.SetValue("console", new CustomConsole());

            if (script.StartsWith("<script") && script.Contains("src="))
            {
                // The script is loaded from an external URL
                var startIndex = script.IndexOf("src=") + 5;
                var endIndex = script.IndexOf("\"", startIndex);
                var url = script.Substring(startIndex, endIndex - startIndex);

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(url);
                    var content = await response.Content.ReadAsStringAsync();
                    _engine.Execute(content);
                }
            }
            else
            {
                // The script is contained within the HTML page
                script = script.Replace("<script>", "").Replace("</script>", "");
                _engine.Execute(script);
            }
        }
        catch (Jint.Parser.ParserException ex)
        {
            Debug.WriteLine($"Jint ParserException occurred: {ex.Message}");
            throw;
        }
    }

    public class CustomConsole
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
