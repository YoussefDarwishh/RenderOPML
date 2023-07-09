﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace RenderOPML.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public List<FeedItem> FeedItems { get; set; } = new List<FeedItem>();

        public int PageSize { get; } = 5;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages => (int)Math.Ceiling((double)FeedItems.Count / PageSize);

        public IndexModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync(int currentPage = 1)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await FetchXmlContentAsync(httpClient, "https://blue.feedland.org/opml?screenname=dave");

            if (response.IsSuccessStatusCode)
            {
                var xmlContent = await response.Content.ReadAsStringAsync();
                FeedItems = ParseOpmlContent(xmlContent);

                // Ensure that the current page is within the valid range
                CurrentPage = Math.Max(1, Math.Min(currentPage, TotalPages));

                // Determine the range of items for the current page
                var startIndex = (CurrentPage - 1) * PageSize;
                var endIndex = Math.Min(startIndex + PageSize, FeedItems.Count);
                FeedItems = FeedItems.GetRange(startIndex, endIndex - startIndex);

                return Page();
            }
            else
            {
                return RedirectToPage("/Error");
            }
        }


        async Task<HttpResponseMessage> FetchXmlContentAsync(HttpClient httpClient, string url)
        {
            return await httpClient.GetAsync(url);
        }

        List<FeedItem> ParseOpmlContent(string opmlContent)
        {
            var feedItems = new List<FeedItem>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(opmlContent);

            XmlNodeList outlineNodes = doc.GetElementsByTagName("outline");

            foreach (XmlNode outlineNode in outlineNodes)
            {
                string xmlUrl = outlineNode.Attributes["xmlUrl"]?.Value;
                string text = outlineNode.Attributes["text"]?.Value;

                if (!string.IsNullOrEmpty(xmlUrl) && !string.IsNullOrEmpty(text))
                {
                    FeedItem feedItem = new FeedItem
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

        public class FeedItem
        {
            public string XmlUrl { get; set; }
            public string Text { get; set; }
        }
    }
}