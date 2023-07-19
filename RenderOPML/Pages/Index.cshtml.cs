using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Xml;

namespace RenderOPML.Pages;

public class IndexModel : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;
    public List<FeedItemOpml> FeedItems { get; set; } = new();

    public int PageSize { get; } = 5;
    public int CurrentPage { get; set; } = 0;
    public int TotalPages { get; set; } = 0;

    public List<FeedItemOpml> StarredFeeds { get; set; } = new();

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

            var starredFeedsJson = Request.Cookies["StarredFeeds"];
            if (!string.IsNullOrEmpty(starredFeedsJson))
            {
                StarredFeeds = JsonSerializer.Deserialize<List<FeedItemOpml>>(starredFeedsJson);
            }

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
            string htmlUrl = outlineNode.Attributes["htmlUrl"]?.Value;

            if (!string.IsNullOrEmpty(xmlUrl) && !string.IsNullOrEmpty(text))
            {
                FeedItemOpml feedItem = new FeedItemOpml
                {
                    XmlUrl = xmlUrl,
                    Text = text,
                    HtmlUrl = htmlUrl
                };

                feedItems.Add(feedItem);
            }
        }

        return feedItems;
    }

    public bool IsFeedStarred(string xmlUrl)
    {
        return StarredFeeds.Any(feed => feed.XmlUrl == xmlUrl);
    }
}

public class FeedItemOpml
{
    public string XmlUrl { get; set; }
    public string Text { get; set; }
    public string HtmlUrl { get; set; }
}
