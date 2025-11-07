#if ANDROID

using AppoMobi.Specials;
using SoundFlow.Abstracts;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Backends.MiniAudio;
using SoundFlow.Components;
using SoundFlow.Enums;
using SoundFlow.Interfaces;
using SoundFlow.Providers;
using System.Numerics;
using JetBrains.Annotations;
using SoundFlow.Structs;
using Debug = System.Diagnostics.Debug;

namespace Breakout.Game;

/// <summary>
/// Game audio service using SoundFlow for high-performance cross-platform audio management
/// </summary>
public class SoundFlowAudioService : IAudioService
{
    #region CONSTANTS

    /// <summary>
    /// Number of audio channels for sound effects
    /// </summary>
    private const int SOUND_EFFECT_CHANNELS = 8;

    /// <summary>
    /// Sample rate for audio engine (48kHz for high quality)
    /// </summary>
    private const int SAMPLE_RATE = 48000;

    #endregion

    #region SOUND DATA STORAGE

    private readonly Dictionary<string, byte[]> _soundData = new();
    private readonly List<SoundPlayer> _soundEffectPlayers = new();
    private readonly Queue<MemoryStream> _memoryStreamPool = new();
    private readonly object _poolLock = new();

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
            UpdateAllVolumes();
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
            UpdateAllVolumes();
        }
    }

    /// <summary>
    /// Gets whether background music is currently playing
    /// </summary>
    public bool IsBackgroundMusicPlaying => _backgroundMusicPlayer?.State == PlaybackState.Playing;

    #endregion

    #region FIELDS

    private readonly AudioEngine _audioEngine;
    private readonly AudioPlaybackDevice _playbackDevice;
    private readonly Mixer _masterMixer;
    private SoundPlayerBase _backgroundMusicPlayer;
    private float _backgroundMusicVolume = 0.3f;
    private int _currentSoundChannel = 0;
    private bool _disposed = false;

    #endregion

    #region CONSTRUCTOR

    /// <summary>
    /// Initializes a new game audio service using SoundFlow
    /// </summary>
    public SoundFlowAudioService()
    {
        try
        {
            _audioEngine = new MiniAudioEngine();

            var defaultDevice = _audioEngine.PlaybackDevices.FirstOrDefault(x => x.IsDefault);
            var format = AudioFormat.StudioHq;

            _playbackDevice = _audioEngine.InitializePlaybackDevice(defaultDevice, format);
            _masterMixer = _playbackDevice.MasterMixer;

            _playbackDevice.Start();

            Debug.WriteLine($"SoundFlow audio engine initialized with {SAMPLE_RATE}Hz sample rate");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error initializing SoundFlow audio engine: {ex}");
            throw;
        }
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
            byte[] audioData;
            using (var stream = await FileSystem.OpenAppPackageFileAsync(filePath))
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                audioData = memoryStream.ToArray();
            }

            _soundData[soundId] = audioData;

            Debug.WriteLine($"Preloaded sound '{soundId}' from '{filePath}' ({audioData.Length} bytes)");
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
        if (_disposed || _audioEngine == null)
            return;

        try
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

            var memoryStream = GetPooledMemoryStream(audioData);
            var dataProvider = new StreamDataProvider(_audioEngine, AudioFormat.StudioHq, memoryStream);
            var player = new SoundPlayer(_audioEngine, AudioFormat.StudioHq, dataProvider);

            player.Volume = volume;
            player.IsLooping = loop;

            _masterMixer.AddComponent(player);

            _soundEffectPlayers.Add(player);

            player.Play();

            CleanupFinishedPlayers();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error playing sound '{soundId}': {ex}");
        }
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

        float distance = position.Length();
        float attenuatedVolume = volume / (1.0f + distance * 0.1f);
        float balance = Math.Clamp(position.X * 0.5f, -1.0f, 1.0f);

        PlaySound(soundId, attenuatedVolume, balance, loop);
    }

    #endregion

    #region BACKGROUND MUSIC

    /// <summary>
    /// Custom sound player with enhanced end-of-stream handling
    /// </summary>
    public class SFPlayer : SoundPlayerBase
    {
        private readonly ISoundDataProvider _dataProvider;

        /// <summary>
        /// A sound player that plays audio from a data provider
        /// </summary>
        public SFPlayer([NotNull] AudioEngine engine, AudioFormat format, [NotNull] ISoundDataProvider dataProvider,
            string name) : base(engine, format, dataProvider)
        {
            _dataProvider = dataProvider;
            Name = name;
        }

        /// <summary>
        /// Handles the end of stream event
        /// </summary>
        protected override void HandleEndOfStream(Span<float> remainingOutputBuffer, int channels)
        {
            if (!_dataProvider.CanSeek)
            {
                var check = $"{Time} / {Duration}  {LoopStartSamples} / {LoopEndSamples}  |  {LoopEndSeconds}";
                Debug.WriteLine(check);
            }

            base.HandleEndOfStream(remainingOutputBuffer, channels);
        }

        /// <inheritdoc />
        public override string Name { get; set; }
    }

    /// <summary>
    /// Starts background music playback directly from file (streaming, memory-efficient)
    /// </summary>
    /// <param name="filePath">Path to the background music file</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public async void StartBackgroundMusicFromFile(string filePath, float volume = 1.0f)
    {
        StopBackgroundMusic();

        _backgroundMusicVolume = Math.Clamp(volume, 0f, 1f);

        try
        {
            var fileStream = await FileSystem.OpenAppPackageFileAsync(filePath);
            var dataProvider = new StreamDataProvider(_audioEngine, AudioFormat.StudioHq, fileStream);
            _backgroundMusicPlayer = new SFPlayer(_audioEngine, AudioFormat.StudioHq, dataProvider, "background");

            _backgroundMusicPlayer.IsLooping = true;
            UpdateBackgroundMusicVolume();

            _masterMixer.AddComponent(_backgroundMusicPlayer);
            _backgroundMusicPlayer.Play();

            //Debug.WriteLine($"Started background music streaming from '{filePath}'");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error starting background music from file '{filePath}': {ex}");
        }
    }

    /// <summary>
    /// Starts background music playback
    /// </summary>
    /// <param name="soundId">ID of the background music sound</param>
    /// <param name="volume">Volume level (0.0 to 1.0)</param>
    public void StartBackgroundMusic(string soundId, float volume = 0.3f)
    {
        StopBackgroundMusic();

        _backgroundMusicVolume = Math.Clamp(volume, 0f, 1f);

        if (_soundData.TryGetValue(soundId, out var audioData))
        {
            try
            {
                var memoryStream = GetPooledMemoryStream(audioData);
                var dataProvider = new StreamDataProvider(_audioEngine, AudioFormat.StudioHq, memoryStream);
                _backgroundMusicPlayer = new SoundPlayer(_audioEngine, AudioFormat.StudioHq, dataProvider);

                _backgroundMusicPlayer.IsLooping = true;
                UpdateBackgroundMusicVolume();

                _masterMixer.AddComponent(_backgroundMusicPlayer);
                _backgroundMusicPlayer.Play();

                //Debug.WriteLine($"Started background music '{soundId}'");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting background music '{soundId}': {ex}");
            }
        }
        else
        {
            Debug.WriteLine($"Background music '{soundId}' not preloaded");
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
                _masterMixer.RemoveComponent(_backgroundMusicPlayer);
                //Debug.WriteLine("Stopped background music");
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

        if (soundId == "background")
        {
            _backgroundMusicVolume = volume;
            UpdateBackgroundMusicVolume();
            return;
        }

        //Debug.WriteLine($"SetSoundVolume for '{soundId}' - individual sound volume control not implemented");
    }

    /// <summary>
    /// Updates all audio volumes based on master volume and mute state
    /// </summary>
    private void UpdateAllVolumes()
    {
        UpdateBackgroundMusicVolume();

        foreach (var player in _soundEffectPlayers.ToList())
        {
            if (player != null)
            {
                try
                {
                    float effectiveVolume = IsMuted ? 0f : MasterVolume;
                    player.Volume = effectiveVolume;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating sound effect volume: {ex}");
                }
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

    #region MEMORY MANAGEMENT

    /// <summary>
    /// Gets a pooled memory stream to reduce GC pressure
    /// </summary>
    /// <param name="audioData">Audio data to initialize the stream with</param>
    /// <returns>A memory stream containing the audio data</returns>
    private MemoryStream GetPooledMemoryStream(byte[] audioData)
    {
        MemoryStream stream;

        lock (_poolLock)
        {
            if (_memoryStreamPool.Count > 0)
            {
                stream = _memoryStreamPool.Dequeue();
                stream.SetLength(0);
                stream.Position = 0;
            }
            else
            {
                stream = new MemoryStream();
            }
        }

        stream.Write(audioData, 0, audioData.Length);
        stream.Position = 0;
        return stream;
    }

    /// <summary>
    /// Returns a memory stream to the pool for reuse
    /// </summary>
    /// <param name="stream">The stream to return to the pool</param>
    private void ReturnMemoryStreamToPool(MemoryStream stream)
    {
        if (stream == null) return;

        lock (_poolLock)
        {
            if (_memoryStreamPool.Count < 10)
            {
                stream.SetLength(0);
                stream.Position = 0;
                _memoryStreamPool.Enqueue(stream);
            }
            else
            {
                stream.Dispose();
            }
        }
    }

    #endregion

    #region CLEANUP

    /// <summary>
    /// Cleans up finished sound effect players to prevent memory leaks
    /// </summary>
    private void CleanupFinishedPlayers()
    {
        try
        {
            var playersToRemove = new List<SoundPlayer>();

            foreach (var player in _soundEffectPlayers.ToList())
            {
                if (player == null || player.State == PlaybackState.Stopped)
                {
                    playersToRemove.Add(player);
                }
            }

            foreach (var player in playersToRemove)
            {
                _soundEffectPlayers.Remove(player);

                if (player != null)
                {
                    try
                    {
                        _masterMixer.RemoveComponent(player);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error removing sound effect player: {ex}");
                    }
                }
            }

            if (_soundEffectPlayers.Count > SOUND_EFFECT_CHANNELS * 2)
            {
                var oldestPlayers = _soundEffectPlayers.Take(_soundEffectPlayers.Count - SOUND_EFFECT_CHANNELS)
                    .ToList();
                foreach (var player in oldestPlayers)
                {
                    _soundEffectPlayers.Remove(player);
                    if (player != null)
                    {
                        try
                        {
                            player.Stop();
                            _masterMixer.RemoveComponent(player);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error stopping old sound effect player: {ex}");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error during player cleanup: {ex}");
        }
    }

    #endregion

    #region DISPOSAL

    /// <summary>
    /// Cleans up all resources used by the audio service
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            Debug.WriteLine("Disposing SoundFlow audio service...");

            StopBackgroundMusic();

            foreach (var player in _soundEffectPlayers.ToList())
            {
                if (player != null)
                {
                    try
                    {
                        player.Stop();
                        _masterMixer.RemoveComponent(player);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error stopping sound effect player: {ex}");
                    }
                }
            }
            _soundEffectPlayers.Clear();

            _soundData.Clear();

            lock (_poolLock)
            {
                while (_memoryStreamPool.Count > 0)
                {
                    var stream = _memoryStreamPool.Dequeue();
                    stream?.Dispose();
                }
            }

            _playbackDevice?.Stop();
            _playbackDevice?.Dispose();
            _audioEngine?.Dispose();

            Debug.WriteLine("SoundFlow audio service disposed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error disposing SoundFlow audio service: {ex}");
        }
        finally
        {
            _disposed = true;
        }
    }

    #endregion
}

#endif