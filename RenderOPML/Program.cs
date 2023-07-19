using Microsoft.AspNetCore.Mvc;
using RenderOPML.Pages;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();


var app = builder.Build();

app.MapPost("/post-star", (HttpContext context, [FromBody] FeedItemOpml newFeed) =>
{ 
    var starredFeedsJson = context.Request.Cookies["StarredFeeds"];
    List<FeedItemOpml> starredFeeds;

    if (string.IsNullOrEmpty(starredFeedsJson))
    {
        starredFeeds = new();
    }
    else
    {
        starredFeeds = JsonSerializer.Deserialize<List<FeedItemOpml>>(starredFeedsJson);
    }

    starredFeeds.Add(new FeedItemOpml
    {
        XmlUrl = newFeed.XmlUrl,
        Text = newFeed.Text,
        HtmlUrl = newFeed.HtmlUrl
    });

    context.Response.Cookies.Append("StarredFeeds", JsonSerializer.Serialize(starredFeeds));

    return Results.Ok();
});

app.MapPost("/delete-star", (HttpContext context, [FromBody] FeedItemOpml deleteFeed) =>
{
    var starredFeedsJson = context.Request.Cookies["StarredFeeds"];
    List<FeedItemOpml> starredFeeds;

    if (string.IsNullOrEmpty(starredFeedsJson))
    {
        starredFeeds = new();
    }
    else
    {
        starredFeeds = JsonSerializer.Deserialize<List<FeedItemOpml>>(starredFeedsJson);
    }

    var feedToRemove = starredFeeds.FirstOrDefault(feed => feed.XmlUrl == deleteFeed.XmlUrl);
    if (feedToRemove != null)
    {
        starredFeeds.Remove(feedToRemove);
        context.Response.Cookies.Append("StarredFeeds", JsonSerializer.Serialize(starredFeeds));
        return Results.Ok();
    }
    return Results.BadRequest();
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
