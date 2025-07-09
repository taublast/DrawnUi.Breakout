using Plugin.Maui.Audio;

namespace Breakout.Game;

public interface IGameAudioSource : IAudioSource, IDisposable
{
}

/// <summary>
/// Preloaded IAudioSource for sound effects (small files, zero-lag playback)
/// </summary>
internal class GameAudioSource : IGameAudioSource
{
    private readonly byte[] _audioData;
    private MemoryStream _stream;

    public GameAudioSource(byte[] audioData)
    {
        _audioData = audioData ?? throw new ArgumentNullException(nameof(audioData));
        _stream = new MemoryStream(_audioData, false); // Read-only
    }

    public Stream GetAudioStream()
    {
        // Reset position and return the same stream (like DOOM)
        _stream.Position = 0;
        return _stream;
    }

    public void Dispose()
    {
        _stream?.Dispose();
    }
}