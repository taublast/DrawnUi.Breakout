using System.Diagnostics;
using System.Numerics;
using AppoMobi.Specials;
using Plugin.Maui.Audio;

namespace Breakout.Game;

/// <summary>
/// Game audio service using Plugin.Maui.Audio's AudioMixer for professional sound management
/// </summary>
public class AudioMixerService : IDisposable
{
    #region CONSTANTS

    /// <summary>
    /// Number of audio channels for sound effects
    /// </summary>
    private const int SOUND_EFFECT_CHANNELS = 8;

    #endregion

    #region SOUND DATA STORAGE

    // Store preloaded audio sources for use with AudioMixer
    private readonly Dictionary<string, MemoryAudioSource> _soundSources = new();

    #endregion

    #region PROPERTIES

    private float _masterVolume = 1.0f;
    private bool _isMuted;

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

    #endregion

    #region FIELDS

    // Audio management using AudioMixer
    private readonly IAudioManager _audioManager;
    private readonly AudioMixer _audioMixer;

    // Background music tracking
    private IAudioPlayer _backgroundMusicPlayer;
    private float _backgroundMusicVolume = 0.3f;

    // Sound effect management
    private int _currentSoundChannel = 0;

    #endregion

    #region CONSTRUCTOR

    /// <summary>
    /// Initializes a new game audio service using AudioMixer
    /// </summary>
    /// <param name="audioManager">The MAUI Audio manager instance</param>
    public AudioMixerService(IAudioManager audioManager)
    {
        _audioManager = audioManager ?? throw new ArgumentNullException(nameof(audioManager));
        _audioMixer = new AudioMixer(audioManager, SOUND_EFFECT_CHANNELS);
    }

    #endregion

    #region PRELOADING

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

            // Create audio source from data
            var audioSource = new MemoryAudioSource(audioData);
            _soundSources[soundId] = audioSource;

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error preloading '{soundId}': {ex}");
            return false;
        }
    }

    #endregion

    #region SOUND PLAYBACK

    /// <summary>
    /// Plays a sound with standard stereo positioning
    /// </summary>
    /// <param name="soundId">ID of the sound to play</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    /// <param name="balance">Stereo balance (-1.0 to 1.0)</param>
    /// <param name="loop">Whether to loop the sound</param>
    public void PlaySound(string soundId, float volume = 1.0f, float balance = 0.0f, bool loop = false)
    {
        if (_audioMixer == null)
            return;

        Tasks.StartDelayed(TimeSpan.FromMicroseconds(1), () =>
        {
            if (IsMuted)
                volume = 0f;
            else
                volume *= MasterVolume;

            if (!_soundSources.TryGetValue(soundId, out var audioSource))
            {
                Debug.WriteLine($"Sound '{soundId}' not preloaded");
                return;
            }

            try
            {
                // Get next available channel (round-robin)
                int channelIndex = _currentSoundChannel;
                _currentSoundChannel = (_currentSoundChannel + 1) % SOUND_EFFECT_CHANNELS;

                // Play sound on the selected channel
                _audioMixer.Play(channelIndex, audioSource, loop);
                _audioMixer.SetVolume(channelIndex, volume);
                _audioMixer.SetBalance(channelIndex, balance);
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

        var (balance, adjustedVolume) = _audioMixer.PositionInSpace(position, volume, 0f);

        PlaySound(soundId, adjustedVolume, balance, loop);
    }

    #endregion

    #region BACKGROUND MUSIC

    /// <summary>
    /// Starts background music playback
    /// </summary>
    /// <param name="soundId">ID of the background music sound</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void StartBackgroundMusic(string soundId, float volume = 0.3f)
    {
        StopBackgroundMusic(); // Stop any existing background music

        _backgroundMusicVolume = Math.Clamp(volume, 0f, 1f);

        if (_soundSources.TryGetValue(soundId, out var audioSource))
        {
            _backgroundMusicPlayer = _audioManager.CreatePlayer(audioSource.GetAudioStream());
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
            try
            {
                _backgroundMusicPlayer.Stop();
                _backgroundMusicPlayer.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping background music: {ex}");
            }
            finally
            {
                _backgroundMusicPlayer = null;
            }
        }
    }

    #endregion

    #region VOLUME MANAGEMENT

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

        // Note: For sound effects, we can't easily adjust volume for specific sound IDs
        // since the AudioMixer manages channels independently. This would require tracking
        // which sounds are playing on which channels, which adds complexity.
        Debug.WriteLine(
            $"SetSoundVolume for '{soundId}' - individual sound volume control not supported with AudioMixer");
    }

    /// <summary>
    /// Updates the volume of all sound effect channels based on master volume and mute state
    /// </summary>
    private void UpdateAllChannelVolumes()
    {
        float effectiveVolume = IsMuted ? 0f : MasterVolume;

        for (int i = 0; i < SOUND_EFFECT_CHANNELS; i++)
        {
            try
            {
                var channel = _audioMixer.GetChannel(i);
                if (channel.IsPlaying)
                {
                    _audioMixer.SetVolume(i, effectiveVolume);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating channel {i} volume: {ex}");
            }
        }
    }

    /// <summary>
    /// Updates background music volume based on settings
    /// </summary>
    private void UpdateBackgroundMusicVolume()
    {
        if (_backgroundMusicPlayer != null)
        {
            try
            {
                float effectiveVolume = IsMuted ? 0f : _backgroundMusicVolume * MasterVolume;
                _backgroundMusicPlayer.Volume = effectiveVolume;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating background music volume: {ex}");
            }
        }
    }

    #endregion

    #region DISPOSAL

    /// <summary>
    /// Cleans up all resources used by the audio service
    /// </summary>
    public void Dispose()
    {
        StopBackgroundMusic();
        _audioMixer?.Dispose();

        foreach (var audioSource in _soundSources.Values)
        {
            try
            {
                audioSource?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disposing audio source: {ex}");
            }
        }

        _soundSources.Clear();
    }

    #endregion
}