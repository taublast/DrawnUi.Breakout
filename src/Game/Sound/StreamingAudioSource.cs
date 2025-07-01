using System.Diagnostics;
using Plugin.Maui.Audio;

namespace BreakoutGame.Game;

/// <summary>
/// Streaming IAudioSource for background music (large files, loaded on-demand)
/// </summary>
internal class StreamingAudioSource : IGameAudioSource
{
    private readonly string _filePath;
    private Stream _currentStream;

    public StreamingAudioSource(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public Stream GetAudioStream()
    {
        // Create a new stream each time for background music (file streaming)
        // This allows the AudioMixer to handle the music file without loading it all into memory
        try
        {
            return FileSystem.OpenAppPackageFileAsync(_filePath).Result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error opening music file '{_filePath}': {ex}");
            return Stream.Null;
        }
    }

    public void Dispose()
    {
        _currentStream?.Dispose();
    }
}