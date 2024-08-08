namespace FanficScraper.Configurations;

public class BackupConfiguration
{
    public bool EnableBackup { get; set; }
    
    public string EncryptionKey { get; set; }

    public string AgeExecutablePath { get; set; } = "age";

    public IReadOnlyList<string> AdditionalFiles { get; set; } = [];
}