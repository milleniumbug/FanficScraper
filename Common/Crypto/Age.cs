using CliWrap;

namespace Common.Crypto;

public class Age
{
    public static async Task Decrypt(Stream input, Stream output, IEnumerable<string> identityFilePaths, string ageExecutablePath = "age")
    {
        var arguments = new List<string>();
        arguments.Add("--decrypt");
        foreach (var identityFilePath in identityFilePaths)
        {
            arguments.Add("-i");
            arguments.Add(identityFilePath);
        }
        
        var process = Cli.Wrap(ageExecutablePath)
            .WithArguments(arguments)
            .WithStandardInputPipe(PipeSource.FromStream(input))
            .WithStandardOutputPipe(PipeTarget.ToStream(output));

        await process.ExecuteAsync();
    }
    
    public static async Task Encrypt(Stream input, Stream output, IEnumerable<string>? recipients = null, IEnumerable<string>? recipientFilePaths = null, string ageExecutablePath = "age")
    {
        var arguments = new List<string>();
        arguments.Add("--encrypt");
        foreach (var recipient in recipients ?? [])
        {
            arguments.Add("-r");
            arguments.Add(recipient);
        }
        foreach (var recipientFilePath in recipientFilePaths ?? [])
        {
            arguments.Add("-R");
            arguments.Add(recipientFilePath);
        }
        
        var process = Cli.Wrap(ageExecutablePath)
            .WithArguments(arguments)
            .WithStandardInputPipe(PipeSource.FromStream(input))
            .WithStandardOutputPipe(PipeTarget.ToStream(output));

        await process.ExecuteAsync();
    }
}