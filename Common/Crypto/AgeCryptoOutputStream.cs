using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.IO.Pipes;
using CliWrap;

namespace Common.Crypto;

#if false

public class AgeCryptoOutputStream : WritableStreamBase
{
    private readonly Stream ciphertextOutputStream;
    private readonly bool leaveOpen;
    private readonly Stream plaintextStream;
    private CommandTask<CommandResult> commandTask;
    private readonly Stream plaintextReaderStream;

    public AgeCryptoOutputStream(Stream ciphertextOutputStream, IEnumerable<string>? recipients = null, IEnumerable<string>? recipientFilePaths = null, string ageExecutablePath = "age", bool leaveOpen = false)
    {
        this.ciphertextOutputStream = ciphertextOutputStream;
        this.leaveOpen = leaveOpen;
        
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

        var pipe = new Pipe();
        this.plaintextStream = pipe.Writer.AsStream();
        this.plaintextReaderStream = pipe.Reader.AsStream();
        
        var process = Cli.Wrap(ageExecutablePath)
            .WithArguments(arguments)
            .WithStandardInputPipe(PipeSource.FromStream(this.plaintextReaderStream))
            .WithStandardOutputPipe(PipeTarget.ToStream(this.ciphertextOutputStream));

        this.commandTask = process.ExecuteAsync();
    }
    
    public override void Flush()
    {
        this.plaintextStream.Flush();
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        this.plaintextStream.Write(buffer);
    }
    
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await this.WriteAsync(buffer.AsMemory(offset, count), cancellationToken);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        await this.plaintextStream.WriteAsync(buffer, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        this.plaintextStream.Dispose();
        this.plaintextReaderStream.Dispose();
        if (this.commandTask.Task.IsFaulted)
        {
            throw new InvalidDataException();
        }
        if (!leaveOpen)
        {
            this.ciphertextOutputStream.Dispose();
        }
    }
}
#endif