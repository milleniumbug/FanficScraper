using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Web;
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
                category: string.IsNullOrEmpty(meta.Category) ? null : meta.Category.Split(',', StringSplitOptions.TrimEntries),
                characters: string.IsNullOrEmpty(meta.Characters) ? null : meta.Characters.Split(',', StringSplitOptions.TrimEntries),
                genre: string.IsNullOrEmpty(meta.Genre) ? null : meta.Genre.Split(',', StringSplitOptions.TrimEntries),
                relationships: string.IsNullOrEmpty(meta.Relationships) ? null : meta.Relationships.Split(',', StringSplitOptions.TrimEntries),
                rating: string.IsNullOrEmpty(meta.Rating) ? null : meta.Rating,
                warnings: string.IsNullOrEmpty(meta.Warnings) ? null : meta.Warnings.Split(',', StringSplitOptions.TrimEntries),
                descriptionParagraphs: MakeDescriptionParagraphs(meta.DescriptionHTML));
        });
    }

    //[return: NotNullIfNotNull("descriptionHtml")]
    private IReadOnlyList<string>? MakeDescriptionParagraphs(string? descriptionHtml)
    {
        if (descriptionHtml == null)
        {
            return null;
        }

        return null; // TODO
    }
}