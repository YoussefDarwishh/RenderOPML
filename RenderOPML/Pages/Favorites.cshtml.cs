using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace RenderOPML.Pages;

public class FavoritesModel : PageModel
{
    public List<FeedItemOpml> FavoriteFeedItems { get; set; } = new List<FeedItemOpml>();

    public void OnGet()
    {
        var starredFeedsJson = Request.Cookies["StarredFeeds"];
        if (!string.IsNullOrEmpty(starredFeedsJson))
        {
            var starredFeeds = JsonSerializer.Deserialize<List<FeedItemOpml>>(starredFeedsJson);
            FavoriteFeedItems = starredFeeds.ToList();
        }
    }
}