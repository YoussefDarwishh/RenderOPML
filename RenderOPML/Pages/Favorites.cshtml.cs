using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace RenderOPML.Pages;

public class FavoritesModel : PageModel
{
    // New properties for favorite feeds
    public List<string> FavoriteXmlUrls { get; set; } = new List<string>();
    public List<string> FavoriteFeedTitles { get; set; } = new List<string>();
    public List<string> FavoriteHtmlUrls { get; set; } = new List<string>();

    public void OnGet()
    {
        // Retrieve favorite feed information from cookies
        if (Request.Cookies["XmlUrl"] is not null && Request.Cookies["FeedTitle"] is not null && Request.Cookies["HtmlUrl"] is not null)
        {
            FavoriteXmlUrls = Request.Cookies["XmlUrl"].Split(',').ToList();
            FavoriteFeedTitles = Request.Cookies["FeedTitle"].Split(',').ToList();
            FavoriteHtmlUrls = Request.Cookies["HtmlUrl"].Split(',').ToList();
        }
    }
}
