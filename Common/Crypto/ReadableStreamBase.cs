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