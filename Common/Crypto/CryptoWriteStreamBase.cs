using System.Runtime.InteropServices;
using System.Text.Json;
using Geralt;

namespace Common.Crypto;

public abstract class CryptoWriteStreamBase<THeader> : WritableStreamBase
{
    private readonly Stream stream;
    private readonly bool leaveOpen;
    private bool headerWritten = false;
    private IncrementalXChaCha20Poly1305 cipher;
    private const int bufferSize = 64 * 1024;
    private byte[] ciphertextBuffer = new byte[bufferSize + IncrementalXChaCha20Poly1305.TagSize];
    private byte[] plaintextBuffer = new byte[bufferSize];
    private Memory<byte> writeArea;
    private THeader header;
    
    protected abstract ReadOnlyMemory<byte> DeriveKey(string passphrase);

    protected abstract THeader StoreNonce(ReadOnlyMemory<byte> nonce);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            FlushFinalBlock();
            if (!leaveOpen)
            {
                this.stream.Dispose();
            }
        }
    }

    public void FlushFinalBlock()
    {
        WriteHeader();

        var finalPlainBuffer = plaintextBuffer.AsSpan(0, plaintextBuffer.Length - writeArea.Length);
        var finalCipherBuffer =
            this.ciphertextBuffer.AsSpan(0, finalPlainBuffer.Length + IncrementalXChaCha20Poly1305.TagSize);
        this.cipher.Push(finalCipherBuffer, finalPlainBuffer, IncrementalXChaCha20Poly1305.ChunkFlag.Final);
        this.stream.Write(finalCipherBuffer);
        this.stream.Flush();
        this.plaintextBuffer.AsSpan().Fill(0);
        this.ciphertextBuffer.AsSpan().Fill(0);
    }

    private void WriteHeader()
    {
        if (!headerWritten)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(header);
            var size = BitConverter.GetBytes(bytes.Length);
            stream.Write(size);
            stream.Write(bytes);
            headerWritten = true;
        }
    }
    
    public override void Flush()
    {
        this.stream.Flush();
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        WriteHeader();
        while (buffer.Length > 0)
        {
            int size = Math.Min(buffer.Length, this.writeArea.Length);
            var chunk = buffer.Slice(0, size);
            buffer = buffer.Slice(size);
            chunk.CopyTo(this.writeArea.Span);
            this.writeArea = this.writeArea.Slice(size);
            WriteBlock();
        }
    }

    private void WriteBlock()
    {
        if (this.writeArea.Length == 0)
        {
            this.cipher.Push(this.ciphertextBuffer, this.plaintextBuffer);
            this.stream.Write(this.ciphertextBuffer);
            this.writeArea = this.plaintextBuffer;
        }
    }

    public CryptoWriteStreamBase(Stream stream, string password, bool leaveOpen = false)
    {
        this.stream = stream;
        this.leaveOpen = leaveOpen;

        var key = DeriveKey(password);

        var nonce = new byte[IncrementalXChaCha20Poly1305.HeaderSize];
        this.cipher = new IncrementalXChaCha20Poly1305(
            decryption: false,
            nonce,
            key.Span);

        this.header = StoreNonce(nonce);
        this.writeArea = this.plaintextBuffer.AsMemory(0, bufferSize);
    }
}