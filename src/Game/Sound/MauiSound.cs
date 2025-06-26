using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Plugin.Maui.Audio;

namespace BreakoutGame.Game;

/// <summary>
/// Simple and reliable game audio service with spatial capabilities
/// </summary>
public class GameAudioService : IDisposable
{
    /// <summary>
    /// Maximum number of simultaneous sound effects to prevent audio spam
    /// </summary>
    private const int MAX_CONCURRENT_SOUNDS = 8;

    #region SOUND DATA STORAGE

    // Store preloaded audio data for creating players on-demand
    private readonly Dictionary<string, byte[]> _soundData = new();

    // Track active sound effect players with their creation time for cleanup and limiting
    private readonly ConcurrentDictionary<IAudioPlayer, DateTime> _activePlayers = new();

    /// <summary>
    /// Cleans up finished sound effect players
    /// </summary>
    private void CleanupFinishedPlayers()
    {
        var playersToRemove = new List<IAudioPlayer>();
        var now = DateTime.Now;

        foreach (var kvp in _activePlayers)
        {
            var player = kvp.Key;
            var createdTime = kvp.Value;

            try
            {
                // Check if player is still playing, but with safety checks
                if (!player.IsPlaying || (now - createdTime).TotalSeconds > 10) // Max 10 seconds per sound
                {
                    playersToRemove.Add(player);
                }
            }
            catch (Exception)
            {
                // If we can't check IsPlaying, assume it's finished and remove it
                playersToRemove.Add(player);
            }
        }

        // Just remove from tracking - don't try to stop/dispose manually
        foreach (var player in playersToRemove)
        {
            _activePlayers.TryRemove(player, out _);
        }
    }

    /// <summary>
    /// Limits the number of concurrent sounds by removing oldest players from tracking
    /// </summary>
    private void EnforceConcurrentSoundLimit()
    {
        CleanupFinishedPlayers();

        if (_activePlayers.Count >= MAX_CONCURRENT_SOUNDS)
        {
            // Find the oldest player by creation time and just remove from tracking
            var oldestEntry = _activePlayers.OrderBy(kvp => kvp.Value).FirstOrDefault();
            if (oldestEntry.Key != null)
            {
                _activePlayers.TryRemove(oldestEntry.Key, out _);
                Debug.WriteLine($"Removed oldest player from tracking to make room for new sound");
            }
        }
    }

    #endregion

    private float _masterVolume = 1.0f;
    private bool _isMuted;

    // Audio management
    private readonly IAudioManager _audioManager;

    // Background music tracking
    private IAudioPlayer _backgroundMusicPlayer;
    private float _backgroundMusicVolume = 0.3f;

    // Spatial audio parameters
    private float _clipDist;
    private float _closeDist;
    private float _attenuator;

    /// <summary>
    /// Gets or sets the master volume for all audio
    /// </summary>
    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = Math.Clamp(value, 0f, 1f);
            UpdateAllChannelVolumes();
        }
    }

    /// <summary>
    /// Gets or sets whether audio is muted
    /// </summary>
    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            _isMuted = value;
            UpdateAllChannelVolumes();
            UpdateBackgroundMusicVolume();
        }
    }

    /// <summary>
    /// Gets whether background music is currently playing
    /// </summary>
    public bool IsBackgroundMusicPlaying => _backgroundMusicPlayer?.IsPlaying == true;

    /// <summary>
    /// Initializes a new game audio service
    /// </summary>
    /// <param name="audioManager">The MAUI Audio manager instance</param>
    public GameAudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
        InitializeSpatialAudioParameters();
    }

    /// <summary>
    /// Gets the current number of active audio players
    /// </summary>
    /// <returns>A tuple containing (Playing, Total) player counts</returns>
    public (int Playing, int Total) GetActivePlayerCount()
    {
        CleanupFinishedPlayers();

        int playingCount = 0;
        foreach (var kvp in _activePlayers)
        {
            try
            {
                if (kvp.Key.IsPlaying)
                    playingCount++;
            }
            catch (Exception)
            {
                // Ignore disposed players
            }
        }

        return (playingCount, _activePlayers.Count);
    }

    /// <summary>
    /// Initializes spatial audio parameters for 3D sound positioning
    /// </summary>
    private void InitializeSpatialAudioParameters()
    {
        _clipDist = 1200f;
        _closeDist = 160f;
        _attenuator = _clipDist - _closeDist;
    }

    /// <summary>
    /// Preloads a sound file into memory for immediate playback
    /// </summary>
    /// <param name="soundId">Identifier for the sound</param>
    /// <param name="filePath">Path to the sound file</param>
    /// <returns>True if preloading was successful</returns>
    public async Task<bool> PreloadSoundAsync(string soundId, string filePath)
    {
        try
        {
            // Load audio data into memory
            byte[] audioData;
            using (var stream = await FileSystem.OpenAppPackageFileAsync(filePath))
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                audioData = memoryStream.ToArray();
            }

            // Store audio data for creating players on-demand
            _soundData[soundId] = audioData;

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error preloading '{soundId}': {ex}");
            return false;
        }
    }

    /// <summary>
    /// Plays a sound with standard stereo positioning
    /// </summary>
    /// <param name="soundId">ID of the sound to play</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    /// <param name="balance">Stereo balance (-1.0 to 1.0)</param>
    /// <param name="loop">Whether to loop the sound</param>
    public void PlaySound(string soundId, float volume = 1.0f, float balance = 0.0f, bool loop = false)
    {
        if (IsMuted)
            volume = 0f;
        else
            volume *= MasterVolume;

        if (!_soundData.TryGetValue(soundId, out var audioData))
        {
            Debug.WriteLine($"Sound '{soundId}' not preloaded");
            return;
        }

        // Enforce concurrent sound limit
        EnforceConcurrentSoundLimit();

        Task.Run(() =>
        {
            try
            {
                // Create a new player for this sound
                var player = _audioManager.CreatePlayer(new MemoryStream(audioData));

                player.Balance = balance;
                player.Volume = volume;
                player.Loop = loop;
                player.Play();

                // Add to active players for tracking with creation time
                _activePlayers[player] = DateTime.Now;

                // Auto-cleanup when finished (for non-looping sounds)
                if (!loop)
                {
                    Task.Run(async () =>
                    {
                        var duration = player.Duration > 0
                            ? TimeSpan.FromSeconds(player.Duration + 0.5) // Small buffer
                            : TimeSpan.FromSeconds(5); // Fallback

                        await Task.Delay(duration);

                        // Just remove from tracking - let the player dispose itself naturally
                        _activePlayers.TryRemove(player, out _);
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error playing sound '{soundId}': {ex}");
            }
        });
    }

    /// <summary>
    /// Plays a sound with spatial positioning
    /// </summary>
    /// <param name="soundId">ID of the sound to play</param>
    /// <param name="position">3D position of the sound</param>
    /// <param name="volume">Base volume before spatial adjustments</param>
    /// <param name="loop">Whether to loop the sound</param>
    public void PlaySpatialSound(string soundId, Vector3 position, float volume = 1.0f, bool loop = false)
    {
        if (IsMuted)
            volume = 0f;
        else
            volume *= MasterVolume;

        var (balance, adjustedVolume) = PositionInSpace(position, volume, 0f);

        PlaySound(soundId, adjustedVolume, balance, loop);
    }

    /// <summary>
    /// Calculates the adjusted Balance and Volume based on the 3D position relative to the listener.
    /// </summary>
    /// <param name="position">The 3D position vector of the sound source.</param>
    /// <param name="baseVolume">The base volume before spatial adjustments.</param>
    /// <param name="baseBalance">The base balance before spatial adjustments.</param>
    /// <returns>A tuple containing the adjusted Balance and Volume.</returns>
    private (float Balance, float Volume) PositionInSpace(Vector3 position, float baseVolume, float baseBalance)
    {
        Vector3 forward = new Vector3(0, 0, -1);

        if (Vector3.Distance(position, forward) < 0.001f)
        {
            return (baseBalance, baseVolume);
        }

        float x = position.X;
        float z = position.Z;
        float angle = MathF.Atan2(x, z);
        float balance = MathF.Sin(angle);
        float distance = MathF.Sqrt(x * x + z * z);
        float attenuation = GetDistanceDecay(distance);
        float volume = baseVolume * attenuation;
        float panningEffect = Math.Clamp(1f - (distance / _clipDist), 0f, 1f);

        balance *= panningEffect;
        balance = Math.Clamp(balance, -1f, 1f);

        return (balance, volume);
    }

    /// <summary>
    /// Calculates distance-based attenuation.
    /// </summary>
    /// <param name="dist">Distance from the listener.</param>
    /// <returns>Attenuation factor.</returns>
    private float GetDistanceDecay(float dist)
    {
        if (dist < _closeDist)
        {
            return 1f;
        }
        else
        {
            return Math.Max((_clipDist - dist) / _attenuator, 0f);
        }
    }

    /// <summary>
    /// Stops all sound playback by clearing tracking (players will finish naturally)
    /// </summary>
    public void StopAllSounds()
    {
        // Just clear tracking - let players finish naturally to avoid COM exceptions
        var count = _activePlayers.Count;
        _activePlayers.Clear();
        Debug.WriteLine($"Cleared tracking for {count} active sound players");
    }

    /// <summary>
    /// Stops playback of a specific sound
    /// </summary>
    /// <param name="soundId">ID of the sound to stop</param>
    public void StopSound(string soundId)
    {
        // Special handling for background music
        if (soundId == "background")
        {
            StopBackgroundMusic();
            return;
        }

        // Note: In the new simple system, we can't easily stop specific sound IDs
        // since each player is independent. This method now stops all sound effects.
        // For more granular control, you'd need to track sound IDs per player.
        StopAllSounds();
    }

    /// <summary>
    /// Sets the volume for a specific sound type
    /// </summary>
    /// <param name="soundId">ID of the sound</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void SetSoundVolume(string soundId, float volume)
    {
        volume = Math.Clamp(volume, 0f, 1f);

        // Special handling for background music
        if (soundId == "background")
        {
            _backgroundMusicVolume = volume;
            UpdateBackgroundMusicVolume();
            return;
        }

        // Note: In the new simple system, we can't easily adjust volume for specific sound IDs
        // since each player is independent and short-lived. This would require tracking
        // sound IDs per player, which adds complexity we're trying to avoid.
        Debug.WriteLine($"SetSoundVolume for '{soundId}' not supported in simplified audio system");
    }

    /// <summary>
    /// Starts background music playback
    /// </summary>
    /// <param name="soundId">ID of the background music sound</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void StartBackgroundMusic(string soundId, float volume = 0.3f)
    {
        StopBackgroundMusic(); // Stop any existing background music

        _backgroundMusicVolume = Math.Clamp(volume, 0f, 1f);

        if (_soundData.TryGetValue(soundId, out var audioData))
        {
            _backgroundMusicPlayer = _audioManager.CreatePlayer(new MemoryStream(audioData));
            _backgroundMusicPlayer.Loop = true;
            UpdateBackgroundMusicVolume();
            _backgroundMusicPlayer.Play();
        }
    }

    /// <summary>
    /// Stops background music playback
    /// </summary>
    public void StopBackgroundMusic()
    {
        if (_backgroundMusicPlayer != null)
        {
            _backgroundMusicPlayer.Stop();
            _backgroundMusicPlayer.Dispose();
            _backgroundMusicPlayer = null;
        }
    }

    /// <summary>
    /// Updates the background music volume based on master volume and mute state
    /// </summary>
    private void UpdateBackgroundMusicVolume()
    {
        if (_backgroundMusicPlayer != null)
        {
            float effectiveVolume = IsMuted ? 0f : _backgroundMusicVolume * MasterVolume;
            _backgroundMusicPlayer.Volume = effectiveVolume;
        }
    }

    /// <summary>
    /// Updates the volume of all channels based on master volume and mute state
    /// </summary>
    private void UpdateAllChannelVolumes()
    {
        float effectiveVolume = IsMuted ? 0f : MasterVolume;

        foreach (var kvp in _activePlayers)
        {
            try
            {
                if (kvp.Key.IsPlaying)
                {
                    kvp.Key.Volume *= effectiveVolume;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating player volume: {ex}");
            }
        }
    }

    /// <summary>
    /// Cleans up all resources used by the audio service
    /// </summary>
    public void Dispose()
    {
        StopBackgroundMusic();

        // Just clear tracking - let sound effect players finish naturally
        var count = _activePlayers.Count;
        _activePlayers.Clear();
        Debug.WriteLine($"Disposed audio service, cleared tracking for {count} players");
    }
}