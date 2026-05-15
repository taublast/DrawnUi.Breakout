#if BROWSER
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;
using System.Globalization;

namespace Breakout.Game;

public partial class WebAudioService : IAudioService
{
    private float _masterVolume = 1.0f;
    private bool _isMuted;
    public string? LastErrorMessage { get; private set; }

    // bg music state tracked in C# — JS is only called when not muted
    private string? _pendingBgId;
    private float _pendingBgVolume;

    public float MasterVolume
    {
        get => _masterVolume;
        set => _masterVolume = Math.Clamp(value, 0f, 1f);
    }

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            bool wasDisabled = _isMuted;
            _isMuted = value;

            if (value)
            {
                try { Interop.StopBg(); } catch { }
            }
            else if (wasDisabled && _pendingBgId != null)
            {
                try { Interop.StartBg(_pendingBgId, _pendingBgVolume * _masterVolume); } catch { }
                try { Interop.ResumeCtx(); } catch { }
            }
        }
    }

    public bool IsBackgroundMusicPlaying
    {
        get { try { return !_isMuted && Interop.IsBgPlaying(); } catch { return false; } }
    }

    public async Task<bool> TryResumeAfterUserGestureAsync()
    {
        LastErrorMessage = null;
        try { Interop.ResumeCtx(); } catch { }
        await Task.Delay(150);

        try
        {
            var state = Interop.GetState();
            if (string.Equals(state, "running", StringComparison.OrdinalIgnoreCase))
                return true;

            var error = Interop.GetLastError();
            LastErrorMessage = !string.IsNullOrWhiteSpace(error)
                ? error
                : string.Format(CultureInfo.InvariantCulture,
                    "AudioContext stayed in '{0}' state after user gesture.", state);
        }
        catch (Exception ex)
        {
            LastErrorMessage = ex.Message;
        }
        return false;
    }

    public async Task<bool> PreloadSoundAsync(string soundId, string filePath)
    {
        LastErrorMessage = null;

        try
        {
            var loaded = await Interop.Preload(soundId, "/" + filePath);
            if (!loaded)
            {
                LastErrorMessage = Interop.GetLastError();
            }

            return loaded;
        }
        catch (Exception ex)
        {
            LastErrorMessage = $"{ex.GetType().Name}: {ex.Message}";
            return false;
        }
    }

    public void PlaySound(string soundId, float volume = 1.0f, float balance = 0.0f, bool loop = false)
    {
        if (_isMuted) return;
        try { Interop.Play(soundId, volume * _masterVolume, balance, loop); } catch { }
    }

    public void PlaySpatialSound(string soundId, Vector3 position, float volume = 1.0f, bool loop = false)
    {
        if (_isMuted) return;
        float balance = Math.Clamp(position.X, -1f, 1f);
        try { Interop.Play(soundId, volume * _masterVolume, balance, loop); } catch { }
    }

    public void StartBackgroundMusic(string soundId, float volume = 0.3f)
    {
        _pendingBgId = soundId;
        _pendingBgVolume = volume;
        if (_isMuted) return;
        try { Interop.StartBg(soundId, volume * _masterVolume); } catch { }
    }

    public void StartBackgroundMusicFromFile(string filePath, float volume = 1.0f)
    {
        // Not supported — browser requires preloaded buffers via PreloadSoundAsync
    }

    public void StopBackgroundMusic()
    {
        _pendingBgId = null;
        _pendingBgVolume = 0f;
        if (_isMuted) return;
        try { Interop.StopBg(); } catch { }
    }

    public void SetSoundVolume(string soundId, float volume)
    {
        if (_isMuted) return;
        if (soundId == "background")
            try { Interop.SetBgVolume(Math.Clamp(volume * _masterVolume, 0f, 1f)); } catch { }
    }

    public void Dispose()
    {
        _pendingBgId = null;
        try { Interop.StopBg(); } catch { }
    }

    private static partial class Interop
    {
        [JSImport("globalThis.breakoutAudio.preload")]
        public static partial Task<bool> Preload(string id, string url);

        [JSImport("globalThis.breakoutAudio.play")]
        public static partial void Play(string id, float volume, float balance, bool loop);

        [JSImport("globalThis.breakoutAudio.startBg")]
        public static partial void StartBg(string id, float volume);

        [JSImport("globalThis.breakoutAudio.stopBg")]
        public static partial void StopBg();

        [JSImport("globalThis.breakoutAudio.setBgVolume")]
        public static partial void SetBgVolume(float volume);

        [JSImport("globalThis.breakoutAudio.resumeCtx")]
        public static partial void ResumeCtx();

        [JSImport("globalThis.breakoutAudio.isBgPlaying")]
        public static partial bool IsBgPlaying();

        [JSImport("globalThis.breakoutAudio.getLastError")]
        public static partial string GetLastError();

        [JSImport("globalThis.breakoutAudio.getState")]
        public static partial string GetState();
    }
}
#endif
