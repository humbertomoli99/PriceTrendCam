using HtmlAgilityPack;
using Jint;

namespace PriceTrendCam.Core.Services;
public class ScriptExecutorService
{
    public static void Main(string[] args)
    {
        // Get the HTML of the website
        var html = GetHtmlFromSite("https://www.example.com").Result;

        // Create an HTML document and load the website's HTML
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        // Get the root node of the HTML document
        var siteNode = GetRootOfHtml(doc);

        // Execute the website's scripts
        ExecuteSiteScripts(siteNode).Wait();
    }

    public static async Task<string> GetHtmlFromSite(string url)
    {
        // Create an HTTP client and get the website's HTML
        using (var httpClient = new System.Net.Http.HttpClient())
        {
            var response = await httpClient.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }
    }

    public static HtmlNode GetRootOfHtml(HtmlDocument doc)
    {
        return doc.DocumentNode;
    }

    public static List<string> GetScriptsFromHtmlNode(HtmlNode node)
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

    public static async Task ExecuteSiteScripts(HtmlNode siteNode)
    {
        // Get all scripts from the site
        var scripts = GetScriptsFromHtmlNode(siteNode);

        // Create an instance of the Jint engine
        var engine = new Engine();

        // Execute each script in order
        foreach (var script in scripts)
        {
            await ExecuteScriptAsync(script, engine);
        }
    }

    public static async Task<object> ExecuteScriptAsync(string script, Engine engine)
    {
        // Provide a custom implementation of the console object to Jint
        engine.SetValue("console", new CustomConsole());

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
                return engine.Execute(content).GetCompletionValue();
            }
        }
        else
        {
            // The script is contained within the HTML page
            script = script.Replace("<script>", "").Replace("</script>", "");
            return engine.Execute(script).GetCompletionValue();
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
