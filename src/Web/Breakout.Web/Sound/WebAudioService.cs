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
                // truly stop — no volume-0 trick
                Interop.StopBg();
            }
            else if (wasDisabled && _pendingBgId != null)
            {
                // re-enable: restart bg music and unblock ctx if needed
                Interop.StartBg(_pendingBgId, _pendingBgVolume * _masterVolume);
                Interop.ResumeCtx();
            }
        }
    }

    public bool IsBackgroundMusicPlaying => !_isMuted && Interop.IsBgPlaying();

    public async Task<bool> TryResumeAfterUserGestureAsync()
    {
        LastErrorMessage = null;
        Interop.ResumeCtx();
        await Task.Delay(150);

        var state = Interop.GetState();
        if (string.Equals(state, "running", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var error = Interop.GetLastError();
        LastErrorMessage = !string.IsNullOrWhiteSpace(error)
            ? error
            : string.Format(CultureInfo.InvariantCulture,
                "AudioContext stayed in '{0}' state after user gesture.", state);
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
        if (_isMuted) return; // zero JS — no call at all
        Interop.Play(soundId, volume * _masterVolume, balance, loop);
    }

    public void PlaySpatialSound(string soundId, Vector3 position, float volume = 1.0f, bool loop = false)
    {
        if (_isMuted) return; // zero JS
        // X is already normalized to ~[-1..1]: -1=left wall, +1=right wall
        float balance = Math.Clamp(position.X, -1f, 1f);
        Interop.Play(soundId, volume * _masterVolume, balance, loop);
    }

    public void StartBackgroundMusic(string soundId, float volume = 0.3f)
    {
        _pendingBgId = soundId;
        _pendingBgVolume = volume;
        if (_isMuted) return; // zero JS — state saved, will restart on unmute
        Interop.StartBg(soundId, volume * _masterVolume);
    }

    public void StartBackgroundMusicFromFile(string filePath, float volume = 1.0f)
    {
        // Not supported — browser requires preloaded buffers via PreloadSoundAsync
    }

    public void StopBackgroundMusic()
    {
        _pendingBgId = null;
        _pendingBgVolume = 0f;
        if (_isMuted) return; // already stopped (StopBg was called on mute)
        Interop.StopBg();
    }

    public void SetSoundVolume(string soundId, float volume)
    {
        if (_isMuted) return; // zero JS
        if (soundId == "background")
            Interop.SetBgVolume(Math.Clamp(volume * _masterVolume, 0f, 1f));
    }

    public void Dispose()
    {
        _pendingBgId = null;
        Interop.StopBg();
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
