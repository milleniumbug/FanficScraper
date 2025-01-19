using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Web;
using Common;
using Common.Challenges;
using Common.Scraping;
using Common.Utils;
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

    public async Task<Predicate<Uri>> GetSupportedSiteMatcher()
    {
        var psi = new ProcessStartInfo()
        {
            FileName = settings.FanFicFareExecutablePath ?? "fanficfare",
            RedirectStandardError = true,
            UseShellExecute = false,
            ArgumentList = { "https://example.com" }
        };
        using var process = new Process()
        {
            StartInfo = psi
        };
        process.Start();

        HashSet<string>? sites = null;

        string? line;
        var splitValues = new[] { ',', ' ', ')', '(' };
        while ((line = await process.StandardError.ReadLineAsync()) != null)
        {
            if (line.TryTrimStart("fanficfare.exceptions.UnknownSite: Unknown Site(https://example.com).  Supported sites: (", StringComparison.Ordinal, out var trimmed))
            {
                var urls = trimmed.Split(splitValues,
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                sites = urls
                    .Select(url =>
                        UrlNoWww(url))
                    .ToHashSet();
            }
        }
        
        await process.WaitForExitAsync();

        if (sites == null)
        {
            throw new FanficFareException("fanficfare format error");
        }

        return uri =>
        {
            var host = uri.Host;
            return sites.Contains(UrlNoWww(host));
        };

        string UrlNoWww(string url)
        {
            return url.TryTrimStart("www.", StringComparison.Ordinal, out var urlNoWww) ? urlNoWww : url;
        }
    }
    
    public Task<FanFicStoryDetails> Run(Uri storyUrl, bool metadataOnly = false, bool force = false)
    {
        return Task.Run(async () =>
        {
            string? cookiesFilePath = null;
            var psi = new ProcessStartInfo()
            {
                FileName = settings.FanFicFareExecutablePath ?? "fanficfare",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            if (settings.ProxyUrl != null)
            {
                psi.EnvironmentVariables["HTTPS_PROXY"] = settings.ProxyUrl;
            }
            
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

            ChallengeSolution? solved = null;
            if (settings.ChallengeSolver != null)
            {
                solved = await settings.ChallengeSolver.Solve(storyUrl);

                if (solved.UserAgent != null)
                {
                    psi.ArgumentList.Add("-o");
                    psi.ArgumentList.Add($"user_agent={solved.UserAgent}");
                }

                if (solved.Cookies != null)
                {
                    cookiesFilePath = Path.GetTempFileName();
                    await using var cookiesFs = File.Create(cookiesFilePath);
                    CookieUtils.WriteCookiesInMozillaFormat(cookiesFs, solved.Cookies);
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
            psi.ArgumentList.Add(storyUrl.ToString());
            try
            {
                var cmdLog = $"{psi.FileName} {string.Join(" ", psi.ArgumentList)}";
                logger.LogInformation("Launching fanficfare with: {CmdLog}", cmdLog);
                using var process = new Process()
                {
                    StartInfo = psi
                };
                process.Start();

                var stdoutReadTask = process.StandardOutput.ReadToEndAsync();
                var stderrReadTask = process.StandardError.ReadToEndAsync();

                await process.WaitForExitAsync();
                var exitCode = process.ExitCode;

                if (exitCode == 0)
                {
                    var meta = JsonSerializer.Deserialize<FanFicFareStoryJson>(await stdoutReadTask)
                               ?? throw new JsonException();
                    
                    this.logger.LogWarning("StdErr output: {StdErr}", await stderrReadTask);

                    return new FanFicStoryDetails(
                        author: HttpUtility.HtmlDecode(meta.Author),
                        license: null,
                        authorId: HttpUtility.HtmlDecode(meta.AuthorId),
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
                else
                {
                    await stdoutReadTask;
                    var stderr = await stderrReadTask;
                    if (stderr.Contains("403 Client Error: Forbidden for url", StringComparison.Ordinal))
                    {
                        if (settings.ChallengeSolver != null && solved != null)
                        {
                            settings.ChallengeSolver.Invalidate(solved);
                        }
                        
                        await Task.Delay(TimeSpan.FromSeconds(10));
                    }
                    throw new FanficFareException(stderr);
                }
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

    private DateTime ParseDate(string s)
    {
        DateTime date;
        var formatsToTry = new[]
        {
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-dd HH:mm"
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
            return document.DocumentNode.GetInnerTextForReal().Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Attempt at parsing the description failed");
            return null;
        }
    }
}