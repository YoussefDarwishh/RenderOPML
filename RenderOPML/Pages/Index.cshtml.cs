using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace RenderOPML.Pages;
public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<FeedItemOpml> FeedItems { get; set; } = new List<FeedItemOpml>();

    public int PageSize { get; } = 5;
    public int CurrentPage { get; set; } = 0;
    public int TotalPages { get; set; } = 0;

    public IndexModel(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> OnGetAsync(int currentPage = 1)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync("https://blue.feedland.org/opml?screenname=dave");

        if (response.IsSuccessStatusCode)
        {
            var xmlContent = await response.Content.ReadAsStringAsync();
            var allFeedItems = ParseOpmlContent(xmlContent);

            var startIndex = (currentPage - 1) * PageSize;
            var endIndex = Math.Min(startIndex + PageSize, allFeedItems.Count);
            FeedItems = allFeedItems.GetRange(startIndex, endIndex - startIndex);

            TotalPages = (int)Math.Ceiling((double)allFeedItems.Count / PageSize);

            CurrentPage = Math.Max(1, Math.Min(currentPage, TotalPages));

            return Page();
        }
        else
        {
            return RedirectToPage("/Error");
        }
    }

    List<FeedItemOpml> ParseOpmlContent(string opmlContent)
    {
        var feedItems = new List<FeedItemOpml>();
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(opmlContent);

        XmlNodeList outlineNodes = doc.GetElementsByTagName("outline");

        foreach (XmlNode outlineNode in outlineNodes)
        {
            string xmlUrl = outlineNode.Attributes["xmlUrl"]?.Value;
            string text = outlineNode.Attributes["text"]?.Value;

            if (!string.IsNullOrEmpty(xmlUrl) && !string.IsNullOrEmpty(text))
            {
                FeedItemOpml feedItem = new FeedItemOpml
                {
                    XmlUrl = xmlUrl,
                    Text = text
                };

                feedItems.Add(feedItem);
            }
        }

        return feedItems;
    }

    public IActionResult OnGetRenderXml(string xmlUrl)
    {
        var httpClient = _httpClientFactory.CreateClient();
        var response = httpClient.GetAsync(xmlUrl).Result;

        if (response.IsSuccessStatusCode)
        {
            var xmlContent = response.Content.ReadAsStringAsync().Result;
            return Content(xmlContent, "text/xml");
        }
        else
        {
            return Content("Failed to retrieve XML content.");
        }
    }

    public class FeedItemOpml
    {
        public string XmlUrl { get; set; }
        public string Text { get; set; }
    }
}