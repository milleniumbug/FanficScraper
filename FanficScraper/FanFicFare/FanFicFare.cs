using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Web;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Components.Web;

namespace FanficScraper.FanFicFare;

public class FanFicFare : IFanFicFare
{
    private readonly FanFicFareSettings settings;
    private readonly ILogger<FanFicFare> logger;

    public FanFicFare(FanFicFareSettings settings, ILogger<FanFicFare> logger)
    {
        this.settings = settings;
        this.logger = logger;
    }
    
    public Task<FanFicStoryDetails> Run(string storyUrl, bool metadataOnly = false, bool force = false)
    {
        return Task.Run(() =>
        {
            var psi = new ProcessStartInfo()
            {
                FileName = "fanficfare",
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };
            if (settings.TargetDirectory != null)
            {
                psi.WorkingDirectory = settings.TargetDirectory;
            }
            
            if (settings.IsAdult)
            {
                psi.ArgumentList.Add("-o");
                psi.ArgumentList.Add("is_adult=true");
            }
            
            if (settings.IncludeImages)
            {
                psi.ArgumentList.Add("-o");
                psi.ArgumentList.Add("include_images=true");
            }

            psi.ArgumentList.Add("-o");
            psi.ArgumentList.Add("never_make_cover=true");
            
            if (metadataOnly)
            {
                psi.ArgumentList.Add("--no-output");
            }

            /*if (!force)
            {
                psi.ArgumentList.Add("--update-epub");
            }*/

            psi.ArgumentList.Add("--json-meta");
            psi.ArgumentList.Add("--no-meta-chapters");
            psi.ArgumentList.Add("--");
            psi.ArgumentList.Add(storyUrl);
            using var process = new Process()
            {
                StartInfo = psi
            };
            process.Start();

            var meta = JsonSerializer.Deserialize<FanFicFareStoryJson>(process.StandardOutput.ReadToEnd())
                       ?? throw new JsonException();

            return new FanFicStoryDetails(
                author: HttpUtility.HtmlDecode(meta.Author),
                title: HttpUtility.HtmlDecode(meta.Title),
                publicationDate: DateTime.ParseExact(meta.DatePublished, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                websiteUpdateDate: DateTime.ParseExact(meta.DateUpdated, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                outputFilename: meta.OutputFilename,
                numChapters: int.TryParse(meta.NumChapters.Replace(",", ""), out var numChapters) ? numChapters : null,
                numWords: int.TryParse(meta.NumWords.Replace(",", ""), out var numWords) ? numWords : null,
                siteUrl: meta.Site,
                siteAbbreviation: meta.SiteAbbreviation,
                storyUrl: meta.StoryUrl,
                isCompleted: meta.Status == "Completed",
                category: string.IsNullOrEmpty(meta.Category) ? null : HttpUtility.HtmlDecode(meta.Category).Split(',', StringSplitOptions.TrimEntries),
                characters: string.IsNullOrEmpty(meta.Characters) ? null : HttpUtility.HtmlDecode(meta.Characters).Split(',', StringSplitOptions.TrimEntries),
                genre: string.IsNullOrEmpty(meta.Genre) ? null : HttpUtility.HtmlDecode(meta.Genre).Split(',', StringSplitOptions.TrimEntries),
                relationships: string.IsNullOrEmpty(meta.Relationships) ? null : HttpUtility.HtmlDecode(meta.Relationships).Split(',', StringSplitOptions.TrimEntries),
                rating: string.IsNullOrEmpty(meta.Rating) ? null : HttpUtility.HtmlDecode(meta.Rating),
                warnings: string.IsNullOrEmpty(meta.Warnings) ? null : HttpUtility.HtmlDecode(meta.Warnings).Split(',', StringSplitOptions.TrimEntries),
                descriptionParagraphs: MakeDescriptionParagraphs(meta.DescriptionHTML));
        });
    }
    
    private IReadOnlyList<string>? MakeDescriptionParagraphs(string? descriptionHtml)
    {
        if (string.IsNullOrWhiteSpace(descriptionHtml))
        {
            return null;
        }

        try
        {
            var document = new HtmlDocument();
            document.LoadHtml(descriptionHtml);
            // https://github.com/zzzprojects/html-agility-pack/issues/427
            return HttpUtility.HtmlDecode(document.DocumentNode.InnerText).Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Attempt at parsing the description failed");
            return null;
        }
    }
}