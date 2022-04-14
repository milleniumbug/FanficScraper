using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Web;

namespace FanficScraper.FanFicFare;

public class FanFicFare : IFanFicFare
{
    private readonly FanFicFareSettings settings;

    public FanFicFare(FanFicFareSettings settings)
    {
        this.settings = settings;
    }
    
    public Task<FanFicStoryDetails> Run(string storyUrl, bool metadataOnly = false)
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

            if (metadataOnly)
            {
                psi.ArgumentList.Add("--no-output");
            }

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
                author: meta.Author,
                title: meta.Title,
                publicationDate: DateTime.ParseExact(meta.DatePublished, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                websiteUpdateDate: DateTime.ParseExact(meta.DateUpdated, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                outputFilename: meta.OutputFilename,
                numChapters: meta.NumChapters,
                numWords: meta.NumWords,
                siteUrl: meta.Site,
                siteAbbreviation: meta.SiteAbbreviation,
                storyUrl: meta.StoryUrl,
                isCompleted: meta.Status == "Completed");
        });
    }
}