using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using OpenQA.Selenium.DevTools;
using System.Net.Http.Headers;

namespace PriceTrendCam.Core.Services;
public class WebPageService
{
    private HttpClient _client;
    private IConfiguration _config;
    private IElement _pageContent;

    public WebPageService()
    {
        _config = Configuration.Default.WithDefaultLoader();

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/111.0.0.0 Safari/537.36 Edg/111.0.1661.41");
        _client = client;
    }

    public async Task<IElement> LoadPageAsync(string RequestUri)
    {
        var context = BrowsingContext.New(_config);
        var response = await _client.GetAsync(RequestUri);
        var content = await response.Content.ReadAsStringAsync();

        var document = await context.OpenAsync(req => req.Content(content));
        _pageContent = document.DocumentElement;
        return _pageContent;
    }

    public IElement SelectElement(string selector)
    {
        return _pageContent.QuerySelector(selector);
    }

    public string GetAttributeValue(IElement element, string attributeName)
    {
        return element.GetAttribute(attributeName);
    }
}
