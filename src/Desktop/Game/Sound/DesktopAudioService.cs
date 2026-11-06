using Miniaudio.Net;
using System.Numerics;

namespace Breakout.Game;

public sealed class DesktopAudioService : IAudioService
{
    private const int MaxPooledEffectInstances = 4;

    private readonly object _sync = new();
    private readonly MiniaudioEngine _engine;
    private readonly Dictionary<string, LoadedSound> _loadedSounds = new(StringComparer.OrdinalIgnoreCase);

    private MiniaudioSound? _backgroundMusic;
    private float _masterVolume = 1.0f;
    private bool _isMuted;
    private float _backgroundVolume = 1.0f;

    public DesktopAudioService()
    {
        _engine = MiniaudioEngine.Create();
        ApplyMasterVolumeUnsafe();
    }

    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            lock (_sync)
            {
                _masterVolume = Clamp01(value);
                ApplyMasterVolumeUnsafe();
            }
        }
    }

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            lock (_sync)
            {
                _isMuted = value;
                ApplyMasterVolumeUnsafe();
            }
        }
    }

    public bool IsBackgroundMusicPlaying
    {
        get
        {
            lock (_sync)
            {
                return _backgroundMusic is { State: SoundState.Playing or SoundState.Starting };
            }
        }
    }

    public void StartBackgroundMusicFromFile(string filePath, float volume = 1.0f)
    {
        var resolvedPath = ResolveFilePath(filePath);
        if (resolvedPath is null)
        {
            return;
        }

        lock (_sync)
        {
            StartBackgroundMusicUnsafe(resolvedPath, volume);
        }
    }

    public Task<bool> PreloadSoundAsync(string soundId, string filePath)
    {
        var resolvedPath = ResolveFilePath(filePath);
        if (resolvedPath is null)
        {
            return Task.FromResult(false);
        }

        lock (_sync)
        {
            if (_loadedSounds.ContainsKey(soundId))
            {
                return Task.FromResult(true);
            }

            var loadedSound = new LoadedSound(resolvedPath, IsMusicAsset(filePath));
            _loadedSounds[soundId] = loadedSound;

            if (!loadedSound.PreferStreaming)
            {
                loadedSound.Instances.Add(CreateEffectSoundUnsafe(loadedSound.FilePath));
            }
        }

        return Task.FromResult(true);
    }

    public void PlaySound(string soundId, float volume = 1.0f, float balance = 0.0f, bool loop = false)
    {
        if (IsMuted)
        {
            return;
        }

        lock (_sync)
        {
            if (!_loadedSounds.TryGetValue(soundId, out var loadedSound))
            {
                return;
            }

            var sound = AcquireEffectSoundUnsafe(loadedSound);
            RestartSoundUnsafe(sound);
            sound.Positioning = SoundPositioning.Relative;
            sound.Pan = Math.Clamp(balance, -1.0f, 1.0f);
            sound.Looping = loop;
            sound.Volume = Clamp01(volume * loadedSound.VolumeMultiplier);
            sound.Start();
        }
    }

    public void PlaySpatialSound(string soundId, Vector3 position, float volume = 1.0f, bool loop = false)
    {
        float distance = position.Length();
        float attenuatedVolume = volume / (1.0f + distance * 0.1f);
        float balance = Math.Clamp(position.X * 0.5f, -1.0f, 1.0f);

        PlaySound(soundId, attenuatedVolume, balance, loop);
    }

    public void StartBackgroundMusic(string soundId, float volume = 0.3f)
    {
        lock (_sync)
        {
            if (!_loadedSounds.TryGetValue(soundId, out var loadedSound))
            {
                return;
            }

            StartBackgroundMusicUnsafe(loadedSound.FilePath, volume);
        }
    }

    public void StopBackgroundMusic()
    {
        lock (_sync)
        {
            StopBackgroundMusicUnsafe();
        }
    }

    public void SetSoundVolume(string soundId, float volume)
    {
        lock (_sync)
        {
            if (string.Equals(soundId, "background", StringComparison.OrdinalIgnoreCase))
            {
                _backgroundVolume = Clamp01(volume);
                if (_backgroundMusic is not null)
                {
                    _backgroundMusic.Volume = _backgroundVolume;
                }

                return;
            }

            if (_loadedSounds.TryGetValue(soundId, out var loadedSound))
            {
                loadedSound.VolumeMultiplier = Clamp01(volume);
            }
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            StopBackgroundMusicUnsafe();

            foreach (var loadedSound in _loadedSounds.Values)
            {
                foreach (var sound in loadedSound.Instances)
                {
                    DisposeSoundUnsafe(sound);
                }
            }

            _loadedSounds.Clear();
            _engine.Dispose();
        }
    }

    private void StartBackgroundMusicUnsafe(string filePath, float volume)
    {
        StopBackgroundMusicUnsafe();

        _backgroundVolume = Clamp01(volume);
        _backgroundMusic = _engine.CreateSound(filePath, SoundInitFlags.Stream | SoundInitFlags.Async | SoundInitFlags.Looping);
        _backgroundMusic.Positioning = SoundPositioning.Relative;
        _backgroundMusic.Looping = true;
        _backgroundMusic.Volume = _backgroundVolume;
        _backgroundMusic.Start();
    }

    private void StopBackgroundMusicUnsafe()
    {
        if (_backgroundMusic is null)
        {
            return;
        }

        DisposeSoundUnsafe(_backgroundMusic);
        _backgroundMusic = null;
    }

    private MiniaudioSound AcquireEffectSoundUnsafe(LoadedSound loadedSound)
    {
        foreach (var instance in loadedSound.Instances)
        {
            if (instance.State == SoundState.Stopped)
            {
                return instance;
            }
        }

        if (loadedSound.Instances.Count < MaxPooledEffectInstances)
        {
            var instance = CreateEffectSoundUnsafe(loadedSound.FilePath);
            loadedSound.Instances.Add(instance);
            return instance;
        }

        return loadedSound.Instances[0];
    }

    private MiniaudioSound CreateEffectSoundUnsafe(string filePath)
    {
        return _engine.CreateSound(filePath, SoundInitFlags.Decode | SoundInitFlags.Async);
    }

    private static void RestartSoundUnsafe(MiniaudioSound sound)
    {
        if (sound.State is SoundState.Playing or SoundState.Starting or SoundState.Stopping)
        {
            sound.Stop();
        }

        sound.SeekToStart();
    }

    private void ApplyMasterVolumeUnsafe()
    {
        _engine.Volume = _isMuted ? 0.0f : _masterVolume;
    }

    private static void DisposeSoundUnsafe(MiniaudioSound sound)
    {
        try
        {
            if (sound.State is SoundState.Playing or SoundState.Starting or SoundState.Stopping)
            {
                sound.Stop();
            }
        }
        catch
        {
        }

        sound.Dispose();
    }

    private static string? ResolveFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        if (Path.IsPathRooted(filePath))
        {
            return File.Exists(filePath) ? filePath : null;
        }

        var relativePath = filePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar)
            .TrimStart(Path.DirectorySeparatorChar);

        var baseDirectoryPath = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (File.Exists(baseDirectoryPath))
        {
            return baseDirectoryPath;
        }

        var currentDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath);
        return File.Exists(currentDirectoryPath) ? currentDirectoryPath : null;
    }

    private static bool IsMusicAsset(string filePath)
    {
        var normalizedPath = filePath.Replace('\\', '/');
        return normalizedPath.StartsWith("Music/", StringComparison.OrdinalIgnoreCase);
    }

    private static float Clamp01(float value)
    {
        return Math.Clamp(value, 0.0f, 1.0f);
    }

    private sealed class LoadedSound
    {
        public LoadedSound(string filePath, bool preferStreaming)
        {
            FilePath = filePath;
            PreferStreaming = preferStreaming;
        }

        public string FilePath { get; }

        public bool PreferStreaming { get; }

        public float VolumeMultiplier { get; set; } = 1.0f;

        public List<MiniaudioSound> Instances { get; } = new();
    }
}