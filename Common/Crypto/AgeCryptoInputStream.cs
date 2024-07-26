using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.IO.Pipes;
using CliWrap;

namespace Common.Crypto;

// disabled due to deadlock bugs
#if false

public class AgeCryptoInputStream : ReadableStreamBase
{
    private readonly Stream ciphertextInputStream;
    private readonly string ageExecutablePath;
    private readonly bool leaveOpen;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private readonly CommandTask<CommandResult> commandTask;
    private readonly Stream plaintextStream;
    private readonly Stream plaintextReaderStream;

    public AgeCryptoInputStream(Stream ciphertextInputStream, IEnumerable<string>? identityFilePaths = null, string ageExecutablePath = "age", bool leaveOpen = false)
    {
        this.ciphertextInputStream = ciphertextInputStream;
        this.ageExecutablePath = ageExecutablePath;
        this.leaveOpen = leaveOpen;
        
        var arguments = new List<string>();
        arguments.Add("--decrypt");
        foreach (var identityFilePath in identityFilePaths ?? [])
        {
            arguments.Add("-i");
            arguments.Add(identityFilePath);
        }
        
        var pipe = new Pipe();
        this.plaintextStream = pipe.Writer.AsStream();
        this.plaintextReaderStream = pipe.Reader.AsStream();
        
        var process = Cli.Wrap(ageExecutablePath)
            .WithArguments(arguments)
            .WithStandardInputPipe(PipeSource.FromStream(this.ciphertextInputStream))
            .WithStandardOutputPipe(PipeTarget.ToStream(this.plaintextStream));

        this.commandTask = process.ExecuteAsync(cancellationTokenSource.Token);
    }

    public override int Read(Span<byte> buffer)
    {
        return this.plaintextReaderStream.Read(buffer);
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        return await this.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = new CancellationToken())
    {
        return await this.plaintextReaderStream.ReadAsync(buffer, cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        this.plaintextStream.Dispose();
        this.plaintextReaderStream.Dispose();
        if (!this.commandTask.Task.IsCompletedSuccessfully)
        {
            throw new InvalidDataException();
        }
        if (!leaveOpen)
        {
            this.ciphertextInputStream.Dispose();
        }
    }
}

#endif