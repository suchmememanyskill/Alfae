namespace RemoteDownloaderPlugin.Utils;

public class StreamInterceptor : Stream
{
    private Stream _stream;
    private IProgress<float> _progress;
    private long _contentLength;
    private long _totalRead;

    public StreamInterceptor(Stream stream, IProgress<float> progress, long contentLength)
    {
        _stream = stream;
        _progress = progress;
        _contentLength = contentLength;
    }

    public override void Flush()
        => _stream.Flush();

    public override int Read(byte[] buffer, int offset, int count)
    {
        var bytesRead = _stream.Read(buffer, offset, count);
        _totalRead += bytesRead;
        _progress.Report((float)_totalRead / _contentLength);
        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
        => _stream.Seek(offset, origin);

    public override void SetLength(long value)
        => _stream.SetLength(value);

    public override void Write(byte[] buffer, int offset, int count)
        => _stream.Write(buffer, offset, count);

    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => _stream.CanWrite;
    public override long Length => _stream.Length;

    public override long Position
    {
        get => _stream.Position;
        set => _stream.Position = value;
    }
}