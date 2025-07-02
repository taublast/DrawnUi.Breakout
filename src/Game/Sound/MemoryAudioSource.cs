namespace BreakoutGame.Game;

/// <summary>
/// Simple audio source implementation for memory-based audio data, reuses stream
/// </summary>
internal class MemoryAudioSource : IGameAudioSource
{
    private readonly byte[] _audioData;
    private readonly MemoryStream _reusableStream;

    public MemoryAudioSource(byte[] audioData)
    {
        _audioData = audioData ?? throw new ArgumentNullException(nameof(audioData));
        _reusableStream = new MemoryStream(_audioData, false); // Read-only
    }

    public Stream GetAudioStream()
    {
        // Reset position and return the same stream
        _reusableStream.Position = 0;
        return _reusableStream;
    }

    public void Dispose()
    {
        _reusableStream?.Dispose();
    }
}