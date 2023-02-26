using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Web;
using FanficScraper.FanFicFare.Challenges;
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
        return Task.Run(async () =>
        {
            string? cookiesFilePath = null;
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

            if (settings.ChallengeSolver != null)
            {
                var solved = await settings.ChallengeSolver.Solve(new Uri(storyUrl));

                if (solved.UserAgent != null)
                {
                    psi.ArgumentList.Add("-o");
                    psi.ArgumentList.Add($"user_agent={solved.UserAgent}");
                }

                if (solved.Cookies != null)
                {
                    cookiesFilePath = Path.GetTempFileName();
                    await using var cookiesFs = File.Create(cookiesFilePath);
                    WriteCookiesInMozillaFormat(cookiesFs, solved);
                    psi.ArgumentList.Add($"--mozilla-cookies={cookiesFilePath}");
                }
            }

            /*if (!force)
            {
                psi.ArgumentList.Add("--update-epub");
            }*/

            psi.ArgumentList.Add("--json-meta");
            psi.ArgumentList.Add("--no-meta-chapters");
            psi.ArgumentList.Add("--");
            psi.ArgumentList.Add(storyUrl);
            try
            {
                var cmdLog = $"{psi.FileName} {string.Join(" ", psi.ArgumentList)}";
                logger.LogInformation("Launching fanficfare with: {CmdLog}", cmdLog);
                using var process = new Process()
                {
                    StartInfo = psi
                };
                process.Start();

                var meta = JsonSerializer.Deserialize<FanFicFareStoryJson>(
                               await process.StandardOutput.ReadToEndAsync())
                           ?? throw new JsonException();

                return new FanFicStoryDetails(
                    author: HttpUtility.HtmlDecode(meta.Author),
                    title: HttpUtility.HtmlDecode(meta.Title),
                    publicationDate: ParseDate(meta.DatePublished),
                    websiteUpdateDate: ParseDate(meta.DateUpdated),
                    outputFilename: meta.OutputFilename,
                    numChapters: int.TryParse(meta.NumChapters.Replace(",", ""), out var numChapters)
                        ? numChapters
                        : null,
                    numWords: int.TryParse(meta.NumWords.Replace(",", ""), out var numWords) ? numWords : null,
                    siteUrl: meta.Site,
                    siteAbbreviation: meta.SiteAbbreviation,
                    storyUrl: meta.StoryUrl,
                    isCompleted: string.IsNullOrEmpty(meta.Status) || meta.Status == "Completed",
                    category: string.IsNullOrEmpty(meta.Category)
                        ? null
                        : HttpUtility.HtmlDecode(meta.Category).Split(',', StringSplitOptions.TrimEntries),
                    characters: string.IsNullOrEmpty(meta.Characters)
                        ? null
                        : HttpUtility.HtmlDecode(meta.Characters).Split(',', StringSplitOptions.TrimEntries),
                    genre: string.IsNullOrEmpty(meta.Genre)
                        ? null
                        : HttpUtility.HtmlDecode(meta.Genre).Split(',', StringSplitOptions.TrimEntries),
                    relationships: string.IsNullOrEmpty(meta.Relationships)
                        ? null
                        : HttpUtility.HtmlDecode(meta.Relationships).Split(',', StringSplitOptions.TrimEntries),
                    rating: string.IsNullOrEmpty(meta.Rating) ? null : HttpUtility.HtmlDecode(meta.Rating),
                    warnings: string.IsNullOrEmpty(meta.Warnings)
                        ? null
                        : HttpUtility.HtmlDecode(meta.Warnings).Split(',', StringSplitOptions.TrimEntries),
                    descriptionParagraphs: MakeDescriptionParagraphs(meta.DescriptionHTML));
            }
            finally
            {
                if (cookiesFilePath != null)
                {
                    File.Delete(cookiesFilePath);
                }
            }
        });
    }

    public static void WriteCookiesInMozillaFormat(
        Stream cookiesFs,
        ChallengeSolution solution)
    {
        using var writer = new StreamWriter(cookiesFs, leaveOpen: true)
        {
            NewLine = "\n"
        };
        var cookies = solution.Cookies;
        if (cookies != null)
        {
            writer.WriteLine("# HTTP Cookie File");
            foreach (var cookie in cookies)
            {
                // seems to be irrelevant nowadays, we need to shut up the Python cookiejar assert here
                bool includeSubdomains = cookie.Domain.StartsWith(".", StringComparison.Ordinal);
                writer.WriteLine($"{cookie.Domain}\t{(includeSubdomains ? "TRUE" : "FALSE")}\t{cookie.Path}\t{(cookie.Secure ? "TRUE" : "FALSE")}\t{new DateTimeOffset(cookie.Expires).ToUnixTimeSeconds()}\t{cookie.Name}\t{cookie.Value}");
            }
        }
        writer.Flush();
    }

    private DateTime ParseDate(string s)
    {
        DateTime date;
        var formatsToTry = new[]
        {
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm:ss"
        };
        foreach (var format in formatsToTry)
        {
            if (DateTime.TryParseExact(s, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return date;
            }
        }

        throw new FormatException($"String '{s}' is not a valid datetime");
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