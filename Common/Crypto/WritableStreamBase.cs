namespace Common.Crypto;

public abstract class WritableStreamBase : Stream
{
    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

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
        var input = buffer.AsSpan(offset, count);
        this.Write(buffer);
    }

    public abstract override void Write(ReadOnlySpan<byte> buffer);

    public override void WriteByte(byte value)
    {
        ReadOnlySpan<byte> span = stackalloc byte[1] { value };
        this.Write(span);
    }

    public override bool CanRead => false;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }
}