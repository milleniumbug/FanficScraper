using HtmlAgilityPack;

namespace Common.Scraping;

public class WebDocument
{
    public HtmlDocument Document { get; }

    public string Url { get; }

    public WebDocument(HtmlDocument document, string url)
    {
        Document = document;
        Url = url;
    }
}