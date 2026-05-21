using System.Numerics;

namespace Breakout.Game;

/// <summary>
/// Interface for game audio services to enable easy switching between implementations
/// </summary>
public interface IAudioService : IDisposable
{
    /// <summary>
    /// Gets or sets the master volume for all audio
    /// </summary>
    float MasterVolume { get; set; }

    /// <summary>
    /// Gets or sets whether audio is muted
    /// </summary>
    bool IsMuted { get; set; }

    /// <summary>
    /// Gets whether background music is currently playing
    /// </summary>
    bool IsBackgroundMusicPlaying { get; }

    void StartBackgroundMusicFromFile(string filePath, float volume = 1.0f);

    /// <summary>
    /// Preloads a sound file into memory for immediate playback
    /// </summary>
    /// <param name="soundId">Identifier for the sound</param>
    /// <param name="filePath">Path to the sound file</param>
    /// <returns>True if preloading was successful</returns>
    Task<bool> PreloadSoundAsync(string soundId, string filePath);

    /// <summary>
    /// Plays a sound with standard stereo positioning
    /// </summary>
    /// <param name="soundId">ID of the sound to play</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    /// <param name="balance">Stereo balance (-1.0 to 1.0)</param>
    /// <param name="loop">Whether to loop the sound</param>
    void PlaySound(string soundId, float volume = 1.0f, float balance = 0.0f, bool loop = false);

    /// <summary>
    /// Plays a sound with spatial positioning
    /// </summary>
    /// <param name="soundId">ID of the sound to play</param>
    /// <param name="position">3D position of the sound</param>
    /// <param name="volume">Base volume before spatial adjustments</param>
    /// <param name="loop">Whether to loop the sound</param>
    void PlaySpatialSound(string soundId, Vector3 position, float volume = 1.0f, bool loop = false);

    /// <summary>
    /// Starts background music playback
    /// </summary>
    /// <param name="soundId">ID of the background music sound</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    void StartBackgroundMusic(string soundId, float volume = 0.3f);

    /// <summary>
    /// Stops background music playback
    /// </summary>
    void StopBackgroundMusic();

    /// <summary>
    /// Sets the volume for a specific sound type
    /// </summary>
    /// <param name="soundId">ID of the sound</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    void SetSoundVolume(string soundId, float volume);
}
