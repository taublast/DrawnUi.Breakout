using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Plugin.Maui.Audio;

/// <summary>
/// Game audio service that manages sound playback with spatial capabilities
/// </summary>
public class GameAudioService : IDisposable
{
    /// <summary>
    /// Maximum number of players per sound type, adjust for spamming sounds cases, fire etc
    /// </summary>
    private const int MAX_CHANNELS_PER_SOUND = 5;

    #region CHANNELS POOL

    // Store audio data for each sound to create additional players as needed
    private readonly Dictionary<string, byte[]> _soundData = new();

    // Track when each player started playing to find the oldest one
    private readonly Dictionary<IAudioPlayer, DateTime> _playerStartTimes = new();

    /// <summary>
    /// Gets the current number of players for a specific sound
    /// </summary>
    private int GetPlayerCountForSound(string soundId)
    {
        if (_playerPools.TryGetValue(soundId, out var queue))
        {
            // Count players in the queue plus ones being used (not in queue)
            int inQueueCount = queue.Count;
            int inUseCount = _playerStartTimes.Count(p => p.Key.IsPlaying && IsSoundPlayer(p.Key, soundId));
            return inQueueCount + inUseCount;
        }
        return 0;
    }

    /// <summary>
    /// Checks if a player is used for a specific sound
    /// </summary>
    private bool IsSoundPlayer(IAudioPlayer player, string soundId)
    {
        // Implementation depends on how you can identify which sound a player belongs to
        // This is a placeholder - you might need to maintain a mapping
        return true; // Simplified for this example
    }

    /// <summary>
    /// Finds the oldest player for a specific sound to reuse
    /// </summary>
    private IAudioPlayer FindOldestPlayerForSound(string soundId)
    {
        DateTime oldest = DateTime.MaxValue;
        IAudioPlayer oldestPlayer = null;

        foreach (var pair in _playerStartTimes)
        {
            if (pair.Key.IsPlaying && IsSoundPlayer(pair.Key, soundId) && pair.Value < oldest)
            {
                oldest = pair.Value;
                oldestPlayer = pair.Key;
            }
        }

        return oldestPlayer;
    }

    #endregion

    private float _masterVolume = 1.0f;
    private bool _isMuted;

    // Audio management
    private readonly ConcurrentDictionary<string, ConcurrentQueue<IAudioPlayer>> _playerPools = new();
    private readonly IAudioManager _audioManager;

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
        }
    }

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
    /// Gets the current number of audio players in use (not in queue)
    /// </summary>
    /// <returns>The number of audio players currently being used for playback</returns>
    public (int Playing, int Total) GetActivePlayerCount()
    {
        int totalPlayers = 0;
        int usedPlayers = 0;

        foreach (var pool in _playerPools.Values)
        {
            int poolSize = pool.Count;
            totalPlayers += poolSize;

            // Count players that are not in the queue (being used)
            foreach (var player in pool)
            {
                if (player.IsPlaying)
                {
                    usedPlayers++;
                }
            }
        }

        return (usedPlayers, totalPlayers);
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
    /// <param name="poolSize">Initial number of players to create for this sound (default: 1)</param>
    /// <returns>True if preloading was successful</returns>
    public async Task<bool> PreloadSoundAsync(string soundId, string filePath, int poolSize = 1)
    {
        try
        {
            // Create or get the queue for this sound
            if (!_playerPools.TryGetValue(soundId, out var queue))
            {
                queue = new ConcurrentQueue<IAudioPlayer>();
                _playerPools[soundId] = queue;
            }

            // Load audio data
            byte[] audioData;
            using (var stream = await FileSystem.OpenAppPackageFileAsync(filePath))
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                audioData = memoryStream.ToArray();
            }

            // Store audio data for later use when creating additional players
            _soundData[soundId] = audioData;

            // Create initial player(s)
            for (int i = 0; i < poolSize && i < MAX_CHANNELS_PER_SOUND; i++)
            {
                var player = _audioManager.CreatePlayer(new MemoryStream(audioData));
                queue.Enqueue(player);
            }

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

        // Get or create queue for this sound
        if (!_playerPools.TryGetValue(soundId, out var queue))
        {
            // Sound wasn't preloaded, try to load it on demand
            queue = new ConcurrentQueue<IAudioPlayer>();
            _playerPools[soundId] = queue;

            // Note: We'll create the player below since we need one now
        }

        IAudioPlayer player = null;

        // Try to get an available player from the queue
        if (!queue.TryDequeue(out player))
        {
            // No available player, check if we can create a new one
            int currentCount = GetPlayerCountForSound(soundId);

            if (currentCount < MAX_CHANNELS_PER_SOUND && _soundData.TryGetValue(soundId, out var audioData))
            {
                // Create a new player since we're under the limit
                player = _audioManager.CreatePlayer(new MemoryStream(audioData));
            }
            else
            {
                // We're at the limit, find the oldest playing sound to reuse
                player = FindOldestPlayerForSound(soundId);

                if (player == null)
                {
                    Debug.WriteLine($"Unable to play sound '{soundId}': No available players and couldn't create new one");
                    return;
                }
            }
        }

        Task.Run(() =>
        {
            if (player.IsPlaying)
                player.Stop();

            player.Balance = balance;
            player.Volume = volume;
            player.Loop = loop;
            player.Seek(0);
            player.Play();

            // Remember when we started playing this sound
            _playerStartTimes[player] = DateTime.Now;

            // Automatically enqueue back when playback completes
            Task.Run(async () =>
            {
                var playbackDuration = player.Duration;

                // Ensure we have valid duration; fallback if not available
                var delay = playbackDuration > 0
                    ? TimeSpan.FromSeconds(playbackDuration)
                    : TimeSpan.FromMilliseconds(500);

                await Task.Delay(delay);

                player.Stop(); // Ensure it's stopped before recycling
                queue.Enqueue(player);
            });
        });
    }

    /// <summary>
    /// Plays a sound with spatial positioning
    /// </summary>
    /// <param name="soundId">ID of the sound to play</param>
    /// <param name="position">3D position of the sound</param>
    /// <param name="volume">Base volume before spatial adjustments</param>
    /// <param name="loop">Whether to loop the sound</param>
    /// <param name="allowMultiple">Whether to allow multiple instances of the same sound</param>
    /// <param name="priority">Priority of the sound (higher values = higher priority)</param>
    public void PlaySpatialSound(string soundId, Vector3 position, float volume = 1.0f,
                               bool loop = false, bool allowMultiple = true, int priority = 0)
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
    /// Stops all sound playback
    /// </summary>
    public void StopAllSounds()
    {
        foreach (var pool in _playerPools.Values)
        {
            foreach (var player in pool)
            {
                player.Stop();
            }
        }
    }

/// <summary>
/// Updates the volume of all channels based on master volume and mute state
/// </summary>
private void UpdateAllChannelVolumes()
{
    float effectiveVolume = IsMuted ? 0f : MasterVolume;
    
    foreach (var pool in _playerPools.Values)
    {
        foreach (var player in pool)
        {
            if (player.IsPlaying)
            {
                player.Volume *= effectiveVolume;
            }
        }
    }
}

    /// <summary>
    /// Cleans up all resources used by the audio service
    /// </summary>
    public void Dispose()
    {
        foreach (var pool in _playerPools.Values)
        {
            foreach (var player in pool)
            {
                player.Stop();
                player.Dispose();
            }
        }

        _playerPools.Clear();
    }
}