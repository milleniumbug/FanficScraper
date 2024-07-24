using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Geralt;

namespace Common.Crypto;

public abstract class ReadableStreamBase : Stream
{
    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override void Flush()
    {
        // do nothing
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return this.Read(buffer.AsSpan(offset, count));
    }

    public abstract override int Read(Span<byte> buffer);

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
    
    protected static int TryReadFully(Stream stream, Span<byte> buffer)
    {
        int numBytesToRead = buffer.Length;
        int numBytesRead = 0;
        do
        {
            int n = stream.Read(buffer.Slice(numBytesRead, numBytesToRead));
            if (n == 0)
                return numBytesRead;
            numBytesRead += n;
            numBytesToRead -= n;
        } while (numBytesToRead > 0);

        return numBytesRead;
    }
}

public class CryptoReadStream : ReadableStreamBase
{
    private readonly Stream stream;
    private string? passphrase;
    private readonly bool leaveOpen;
    private IncrementalXChaCha20Poly1305? cipher;
    private const int bufferSize = 64 * 1024;
    private byte[] ciphertextBuffer = new byte[bufferSize + IncrementalXChaCha20Poly1305.TagSize];
    private byte[] plaintextBuffer = new byte[bufferSize];
    private Memory<byte> readArea = Memory<byte>.Empty;
    private FileInfo? fileInfo;

    public CryptoReadStream(Stream stream, string passphrase, bool leaveOpen = false)
    {
        this.stream = stream;
        this.passphrase = passphrase;
        this.leaveOpen = leaveOpen;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.ciphertextBuffer.AsSpan().Fill(0);
            this.plaintextBuffer.AsSpan().Fill(0);
            if (!leaveOpen)
            {
                this.stream.Dispose();
            }
        }
    }

    public override int Read(Span<byte> buffer)
    {
        ReadHeader();

        int bytesToRead = buffer.Length;
        int bytesRead = 0;
        while (bytesToRead > 0)
        {
            if (!FillReadArea())
            {
                return bytesRead;
            }

            int n = Math.Min(this.readArea.Length, bytesToRead);

            this.readArea.Slice(0, n).Span.CopyTo(buffer.Slice(0, n));
            buffer = buffer.Slice(n);
            this.readArea = this.readArea.Slice(n);
            bytesRead += n;
            bytesToRead -= n;
        }
        
        return bytesRead;
    }

    private bool FillReadArea()
    {
        if (readArea.Length == 0)
        {
            var readCount = TryReadFully(stream, ciphertextBuffer);
            if (readCount == 0)
            {
                return false;
            }
            
            var ciphertextChunk = ciphertextBuffer.AsSpan(0, readCount);
            var expectedChunkFlag = readCount != ciphertextBuffer.Length
                ? IncrementalXChaCha20Poly1305.ChunkFlag.Final
                : IncrementalXChaCha20Poly1305.ChunkFlag.Message;

            var plaintextChunk =
                this.plaintextBuffer.AsMemory(0, ciphertextChunk.Length - IncrementalXChaCha20Poly1305.TagSize);
            
            var chunkFlag = this.cipher!.Pull(
                plaintextChunk.Span,
                ciphertextChunk);

            if (chunkFlag != expectedChunkFlag)
            {
                throw new InvalidDataException("possible premature end of stream");
            }

            this.readArea = plaintextChunk;
        }

        return true;
    }

    [MemberNotNull(nameof(fileInfo))]
    private void ReadHeader()
    {
        if (fileInfo == null)
        {
            var headerSizeSpan = this.plaintextBuffer.AsSpan(0, sizeof(int));
            this.stream.ReadExactly(headerSizeSpan);
            var headerSize = BitConverter.ToInt32(headerSizeSpan);

            if (headerSize > bufferSize)
            {
                throw new InvalidDataException();
            }
            
            var headerBytes = this.plaintextBuffer.AsSpan(0, headerSize);
            this.stream.ReadExactly(headerBytes);

            fileInfo = JsonSerializer.Deserialize<FileInfo>(headerBytes)
                       ?? throw new InvalidDataException("can't be null");

            var key = Crypto.DeriveKey(this.passphrase!, fileInfo.DataInfo.KeyInfo);
            this.cipher = new IncrementalXChaCha20Poly1305(
                decryption: true,
                fileInfo.DataInfo.Nonce.ToArray(),
                key.Span);
            this.passphrase = null;
        }
    }
}