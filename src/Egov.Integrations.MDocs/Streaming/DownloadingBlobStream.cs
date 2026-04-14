using System.Net.Http.Headers;
using Egov.Integrations.MDocs.Helpers;

namespace Egov.Integrations.MDocs.Streaming;

internal class DownloadingBlobStream : Stream
{
    private readonly HttpClient _client;
    private readonly Uri _blobUri;

    private long _position;
    private HttpResponseMessage? _currentResponse;
    private Stream? _currentStream;

    public DownloadingBlobStream(HttpClient client, Uri blobUri, HttpResponseMessage initialResponse)
    {
        _client = client;
        _blobUri = blobUri;

        var contentHeaders = initialResponse.Content.Headers;
        Length = contentHeaders.ContentRange?.Length ?? contentHeaders.ContentLength ?? 0L;

        _currentResponse = initialResponse;
    }

    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length { get; }

    public override long Position
    {
        get => _position;
        set
        {
            if ((_position + value < 0) || (_position + value > Length))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _currentStream?.Dispose();
            _currentStream = null;
            _currentResponse?.Dispose();
            _currentResponse = null;

            _position = value;
        }
    }

    public override void Flush() => throw new InvalidOperationException();

    public override long Seek(long offset, SeekOrigin origin)
    {
        return Position = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => _position + offset,
            SeekOrigin.End => Length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };
    }

    public override void SetLength(long value) => throw new InvalidOperationException();

    public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

    public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer, offset, count).Result;

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (buffer.Length - offset < count)
        {
            throw new ArgumentException("Invalid offset length");
        }

        if (count == 0)
        {
            return 0;
        }

        return await ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        while (true)
        {
            if (buffer.IsEmpty)
            {
                return 0;
            }

            if (_currentResponse == null)
            {
                var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, _blobUri)
                {
                    Headers =
                    {
                        Range = new RangeHeaderValue(_position, null)
                    }
                }, cancellationToken);

                // ensure unsuccessful response is disposed
                if (!response.IsSuccessStatusCode)
                {
                    using (response) throw response.CreateExceptionFromBadResponse();
                }

                _currentResponse = response;
            }

            _currentStream ??= await _currentResponse.Content.ReadAsStreamAsync(cancellationToken);

            var result = await _currentStream.ReadAsync(buffer, cancellationToken);
            if (result > 0)
            {
                _position += result;
                return result;
            }

            await _currentStream.DisposeAsync();
            _currentStream = null;
            _currentResponse.Dispose();
            _currentResponse = null;

            if (_position >= Length)
            {
                return 0;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _currentStream?.Dispose();
            _currentResponse?.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_currentStream != null)
        {
            await _currentStream.DisposeAsync();
        }
        _currentResponse?.Dispose();
    }
}