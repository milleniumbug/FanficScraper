using System.Web;
using HtmlAgilityPack;

namespace Common.Scraping;

public static class ScrapingUtils
{
    public static string GetInnerTextForReal(this HtmlNode node)
    {
        // https://github.com/zzzprojects/html-agility-pack/issues/427
        return HttpUtility.HtmlDecode(node.InnerText);
    }
}