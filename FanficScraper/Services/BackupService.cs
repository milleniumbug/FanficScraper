using System.Diagnostics.CodeAnalysis;
using System.Formats.Tar;
using System.IO.Compression;
using System.IO.Pipelines;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Common;
using Common.Api;
using Common.Crypto;
using FanficScraper.Configurations;
using FanficScraper.Data;
using FanficScraper.FanFicFare;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FanficScraper.Services;

public class BackupService
{
    private readonly StoryUpdateLock storyUpdateLock;
    private readonly DataConfiguration dataConfiguration;
    private readonly TimeProvider timeProvider;
    private readonly StoryContext dbContext;
    private readonly FanFicScraperClient? fanFicScraperClient;
    private readonly BackupConfiguration backupConfiguration;

    public BackupService(
        StoryUpdateLock storyUpdateLock,
        IOptions<BackupConfiguration> backupConfiguration,
        IOptions<DataConfiguration> dataConfiguration,
        TimeProvider timeProvider,
        StoryContext dbContext,
        FanFicScraperClient? fanFicScraperClient)
    {
        this.storyUpdateLock = storyUpdateLock;
        this.dataConfiguration = dataConfiguration.Value;
        this.timeProvider = timeProvider;
        this.dbContext = dbContext;
        this.fanFicScraperClient = fanFicScraperClient;
        this.backupConfiguration = backupConfiguration.Value;
    }

    public record BackupRequest(
        bool IncludePrivateData,
        bool IncludeEpubs,
        string? Key,
        DateTimeOffset CurrentTime)
    {
        public bool Encrypted => Key != null;
        
        public string FileName => $"backup-{CurrentTime:yyyyMMddHHmmss}.tar.gz{(Encrypted ? ".enc" : "")}";

        public string MimeType => Key == null ? "application/gzip" : "application/octet-stream";
    }

    public BackupRequest? PrepareBackup(string? key, bool includeEpubs)
    {
        if (!backupConfiguration.EnableBackup)
        {
            return null;
        }

        return new BackupRequest(
            IncludePrivateData: key != null && key == backupConfiguration.EncryptionKey,
            IncludeEpubs: includeEpubs,
            Key: key,
            CurrentTime: this.timeProvider.GetUtcNow());
    }

    public async Task WriteBackup(Stream output, BackupRequest request)
    {
        using var backupLock = await this.storyUpdateLock.TakeBackupCreateLock();

        var tarSink = output;
        var encryptionTask = Task.CompletedTask;
        if (request.Encrypted && request.Key != null)
        {
            var pipe = new Pipe();
            encryptionTask = Age.Encrypt(
                input: pipe.Reader.AsStream(),
                output: output,
                recipients: [request.Key],
                ageExecutablePath: backupConfiguration.AgeExecutablePath);
            tarSink = pipe.Writer.AsStream();
        }

        await using (var gzoStream = new GZipStream(tarSink, CompressionMode.Compress))
        await using (var tarArchive = new TarWriter(gzoStream, TarEntryFormat.Gnu))
        {
            await WriteConfiguration(tarArchive, request);
            await WriteStoriesDirectory(tarArchive, request);
            await WriteDatabase(tarArchive, request);
            await WriteAdditionalFiles(tarArchive, request);
        }
        
        await encryptionTask;
    }

    private async Task WriteSecondaryInstanceBackup(TarWriter tarArchive, BackupRequest request)
    {
        if (!request.IncludePrivateData)
        {
            return;
        }

        if (this.fanFicScraperClient == null)
        {
            return;
        }

        var (_, backupStream) = await this.fanFicScraperClient.Backup(request.Key, includeEpubs: false);
        if (backupStream == null)
        {
            throw new InvalidDataException("could not get from secondary instance");
        }
        var tarEntry = new GnuTarEntry(TarEntryType.RegularFile, "secondary.tar.gz.enc")
        {
            DataStream = backupStream
        };
        
        await tarArchive.WriteEntryAsync(tarEntry);
    }
    
    private async Task WriteAdditionalFiles(TarWriter tarArchive, BackupRequest request)
    {
        var rootDirectoryEntry = new GnuTarEntry(TarEntryType.Directory, "additionalFiles");
        await tarArchive.WriteEntryAsync(rootDirectoryEntry);
        
        if (!request.IncludePrivateData)
        {
            return;
        }

        foreach (var filePath in backupConfiguration.AdditionalFiles)
        {
            if (!Path.IsPathFullyQualified(filePath))
            {
                continue;
            }

            await tarArchive.WriteEntryAsync(FollowSymlink(new FileInfo(filePath)).FullName, rootDirectoryEntry.Name + "/" + Path.GetRelativePath(Path.GetPathRoot(filePath)!, filePath));
        }
    }

    private async Task WriteDatabase(TarWriter tarArchive, BackupRequest request)
    {
        if (!request.IncludePrivateData)
        {
            return;
        }
        
        var backup = Path.GetTempFileName();
        await dbContext.Database.ExecuteSqlInterpolatedAsync($"VACUUM INTO {backup}");
        await tarArchive.WriteEntryAsync(backup, "app.db");
        File.Delete(backup);
    }

    private async Task WriteConfiguration(TarWriter tarArchive, BackupRequest request)
    {
        if (!request.IncludePrivateData)
        {
            return;
        }
        
        var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        foreach (var fileInfo in new DirectoryInfo(directory!).EnumerateFiles("appsettings.*"))
        {
            await tarArchive.WriteEntryAsync(fileInfo.FullName, fileInfo.Name);            
        }
    }
    
    private async Task WriteStoriesDirectory(TarWriter tarArchive, BackupRequest request)
    {
        var directory = dataConfiguration.StoriesDirectory;
        var rootEntryName = Path.GetFileName(dataConfiguration.StoriesDirectory);
        await tarArchive.WriteEntryAsync(directory, rootEntryName);
        foreach (var fileSystemInfo in new DirectoryInfo(directory).EnumerateFileSystemInfos())
        {
            if (fileSystemInfo is FileInfo && fileSystemInfo.Extension != ".epub" && !request.IncludePrivateData)
            {
                continue;
            }
            if (fileSystemInfo is FileInfo && fileSystemInfo.Extension == ".epub" && !request.IncludeEpubs)
            {
                continue;
            }
            var entryName = Path.GetRelativePath(directory, fileSystemInfo.FullName);
            await tarArchive.WriteEntryAsync(fileSystemInfo.FullName, rootEntryName + "/" + entryName);            
        }
    }
    
    private static FileInfo FollowSymlink(FileInfo fileInfo)
    {
        FileSystemInfo fileSystemInfo = fileInfo;
        fileSystemInfo = fileSystemInfo.ResolveLinkTarget(returnFinalTarget: true) ?? fileSystemInfo;
        return (FileInfo)fileSystemInfo;
    }
}