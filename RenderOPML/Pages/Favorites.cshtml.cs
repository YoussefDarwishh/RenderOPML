using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace RenderOPML.Pages;

public class FavoritesModel : PageModel
{
    public List<FeedItemOpml> FavoriteFeedItems { get; set; } = new List<FeedItemOpml>();

    public void OnGet()
    {
        if (Request.Cookies["StarredFeeds"] is not null)
        {
            var starredFeeds = Request.Cookies["StarredFeeds"];
            var starredFeedsList = starredFeeds.Split(',').ToList();

            foreach (var feed in starredFeedsList)
            {
                // Retrieve favorite feed information from cookies
                var feedTitle = Request.Cookies[$"{Uri.EscapeDataString(feed)}_FeedTitle"];
                var htmlUrl = Request.Cookies[$"{Uri.EscapeDataString(feed)}_HtmlUrl"];

                if (!string.IsNullOrEmpty(feedTitle) && !string.IsNullOrEmpty(htmlUrl))
                {
                    FeedItemOpml favoriteFeedItem = new FeedItemOpml
                    {
                        XmlUrl = feed,
                        Text = feedTitle,
                        HtmlUrl = htmlUrl
                    };

                    FavoriteFeedItems.Add(favoriteFeedItem);
                }
            }
        }
    }
}
