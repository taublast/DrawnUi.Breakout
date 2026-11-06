# Audio System

## Interface

`src/Shared/Game/Sound/IAudioService.cs`

```csharp
public interface IAudioService : IDisposable
{
    float MasterVolume { get; set; }
    bool IsMuted { get; set; }
    bool IsBackgroundMusicPlaying { get; }
    void StartBackgroundMusicFromFile(string filePath, float volume);
    Task<bool> PreloadSoundAsync(string soundId, string filePath);
    void PlaySound(string soundId, float volume = 1.0f, float balance = 0.0f, bool loop = false);
    void PlaySpatialSound(string soundId, Vector3 position, float volume = 1.0f, bool loop = false);
    void StartBackgroundMusic(string soundId, float volume = 0.3f);
    void StopBackgroundMusic();
    void SetSoundVolume(string soundId, float volume);
}
```

## Platform Implementations

| Platform | Class | Library |
|----------|-------|---------|
| Android | `SoundFlowAudioService` | SoundFlow + MiniAudio.Net |
| iOS / macOS Catalyst | `AudioMixerService` | Plugin.Maui.Audio |
| Blazor WASM | `WebAudioService` | Web Audio API via JS interop |
| Desktop (OpenTK) | `DesktopAudioService` | Miniaudio.Net |

### Factory (`BreakoutGame.Sound.cs`)

```csharp
private static IAudioService CreateAudioService()
{
#if BROWSER
    return new WebAudioService();
#elif ANDROID
    return new SoundFlowAudioService();
#elif DESKTOP
    return new DesktopAudioService();
#else
    return new AudioMixerService(Plugin.Maui.Audio.AudioManager.Current);
#endif
}
```

---

## Game Sound Layer (`BreakoutGame.Sound.cs`)

### Sound enum

```csharp
public enum Sound {
    None, Board, Brick, Wall, Oops, Start, PowerUp, PowerDown,
    Attack, Dialog, Selection, Joy, Sad
}
```

### Asset list (22 total)

**Effects (WAV):** bip, bip1, bip2, brickglass, bricksynth2, bricksynth3, gamestart, kill, one, pong, tik, two  
**Fx (MP3/WAV):** ballout, bricksynth, powerup27, quirky26, quirky7, synthchime2, bells1  
**Music (MP3):** demoHypnoticPuzzle4, lvl1PixelCityGroovin, MonkeyDrama, TechnoTronic

### Key methods

- `InitializeAudioAsync()` — preloads all 22 assets with progress callback
- `PlaySound(Sound, Vector3)` — queued via `Tasks.StartDelayedAsync()` (1 µs) to avoid blocking
- `PlayMusicLooped(int level)` — level 0 → demo track; level > 0 → play track
- `PlaySpecialMusic()` / `PlaySpeedyMusic()` — switches background track
- `SetupBackgroundMusic()` — reads `AppSettings.MusicOn` and `GameState`
- `ToggleSound()` — flips `IsMuted`
- `EnsureWebAudioRunningAfterGestureAsync()` — Web only, resumes suspended AudioContext

---

## Implementations Detail

### Android — `SoundFlowAudioService`

**File:** `src/Mobile/Game/Sound/SoundFlowAudioService.cs`

- Engine: `MiniAudioEngine` at `AudioFormat.StudioHq` (48 kHz, hardcoded)
- Preload: `FileSystem.OpenAppPackageFileAsync()` → `MemoryStream` → `Dictionary<string, byte[]>`
- Playback: 8 round-robin channels; memory stream pool (max 10) reused to reduce GC
- Auto-cleanup: removes finished players when queue > `SOUND_EFFECT_CHANNELS * 2` (16)
- Background music: streaming via `StreamDataProvider` (large files stay on disk)
- Spatial: `volume / (1.0f + distance * 0.1f)`, balance clamped to [-1, 1]

**Gotchas:**
- 48 kHz hardcoded — mixing sample rates not tested
- Memory pool reduces GC; do not bypass it

### iOS / macOS — `AudioMixerService`

**File:** `src/Mobile/Game/Sound/AudioMixerService.cs`

- Engine: `IAudioManager` (MAUI host) wrapped in custom `AudioMixer` (8 channels)
- Preload: bytes → `MemoryAudioSource` (resets stream position on each `GetAudioStream()` call)
- Playback: round-robin via `AudioMixer.Play(channelIndex, audioSource, loop)`
- Background: `IAudioPlayer` from `MemoryAudioSource.GetAudioStream()` with loop enabled
- Spatial: `AudioMixer.PositionInSpace()` computes balance + attenuation

**Gotchas:**
- `SetSoundVolume()` for individual sound IDs not supported — mixer owns channels, not IDs
- `MemoryAudioSource.GetAudioStream()` resets position on the same object — not a new stream

### Blazor WASM — `WebAudioService`

**File:** `src/Web/Sound/WebAudioService.cs`  
**JS:** `src/Web/wwwroot/breakout-audio.js` (loaded versioned for cache busting)

- All ops delegated to JS via `[JSImport]` (C# → `AudioContext`)
- Preload: `fetch()` + `AudioContext.decodeAudioData()`
- Playback: `BufferSource` → `StereoPanner` → `GainNode` → `destination`
- Background: looping `BufferSource` with dedicated `GainNode`; pending state saved across mute cycles (`_pendingBgId`, `_pendingBgVolume`)
- Spatial: `position.X` clamped to [-1, 1] as pan value

**Autoplay policy handling:**
- `AudioContext` starts suspended until user gesture
- Resumes on `pointerdown`, `visibilitychange`, `pageshow`/`pagehide`
- `TryResumeAfterUserGestureAsync()` checks context state; errors stored in `LastErrorMessage` and shown in UI dialog

**Gotchas:**
- No streaming — all music decoded upfront into buffers
- Tab background → context suspends → pending bg music state replayed on resume
- Must call `NotifyAudioUserGesture()` after first user interaction

### Desktop (OpenTK) — `DesktopAudioService`

**File:** `src/Desktop/Game/Sound/DesktopAudioService.cs`

- Engine: `MiniaudioEngine.Create()`
- Preload:
  - Effects: `SoundInitFlags.Decode | Async` (decoded into memory)
  - Music paths (prefix `"Music/"`) use `PreferStreaming = true` → `SoundInitFlags.Stream | Async | Looping`
- Playback: pool of max 4 instances per sound ID; restarts oldest if limit hit
- Background: single `_backgroundMusic` instance replaced on track change
- Spatial: same attenuation formula as Android
- Thread safety: all ops guarded by `lock (_sync)`

**Path resolution:**
1. Absolute path → use directly
2. Relative → search `AppContext.BaseDirectory`, then current directory

**Settings storage:** `%LOCALAPPDATA%\DrawnUi.Breakout\settings.json`

**Gotchas:**
- Every call must hold `_sync` lock — unsynchronized concurrent calls will corrupt engine state
- Desktop assets must be present at bin output path; check copy rules in `.csproj` if missing

---

## Asset Locations

| Platform | Source path | Format |
|----------|-------------|--------|
| Web | `src/Web/wwwroot/Sounds/`, `Fx/`, `Music/` | WAV + MP3 |
| Mobile | `src/Mobile/Resources/Raw/Sounds/`, `Fx/`, `Music/` | WAV + MP3 |
| Desktop | Copied to `bin/.../Sounds/`, `Fx/`, `Music/` at build | WAV + MP3 |

---

## Settings & Mute Integration

`AppSettings.cs` keys:

| Key | String | Default |
|-----|--------|---------|
| `MusicOn` | `"mus"` | `true` |
| `SoundsOn` | `"fx"` | `true` |

Storage per platform:

| Platform | Storage |
|----------|---------|
| Browser | `localStorage` with `"breakout."` prefix |
| Mobile | `Preferences.Default` (MAUI) |
| Desktop | `%LOCALAPPDATA%\DrawnUi.Breakout\settings.json` |

Volume control flow:
1. `MasterVolume` on `IAudioService` — affects all playback
2. `IsMuted` — suppresses without losing volume state
3. `SetSoundVolume("background", v)` — adjusts background track specifically
4. Game reads settings at startup before `InitializeAudioAsync()`

DemoPlay mode: audio muted unless `USE_SOUND_IN_DEMO = true` (compile-time constant).

---

## Spatial Audio (all platforms)

Common formula:
```csharp
float distance = position.Length();
float attenuatedVolume = volume / (1.0f + distance * 0.1f);
float balance = Math.Clamp(position.X * factor, -1.0f, 1.0f);
```

Default position when not specified: `(1f, 1f, 1f)`.

---

## Startup Flow

1. `BreakoutGame.StartupAssets.cs` calls `InitializeAudioAsync()`
2. All 22 assets preloaded with progress reported to loading screen
3. If `USE_SOUND = false` (compile flag), audio init skipped entirely
4. Load failure → exception shown in game dialog
5. Web: `NotifyAudioUserGesture()` must fire after first user interaction to resume context

---

## Summary

| | Browser | Android | iOS/macOS | Desktop |
|-|---------|---------|-----------|---------|
| Library | Web Audio API | SoundFlow | Plugin.Maui.Audio | Miniaudio.Net |
| Preload | fetch + decode | FileSystem → RAM | FileSystem → RAM | Disk path |
| Channels | BufferSource each | 8 pooled | 8 (AudioMixer) | Pool ≤ 4/sound |
| Music | Buffered only | Stream | Stream | Stream |
| Mute | Stop JS | Mixer volume 0 | Player volume | Engine volume |
| Balance | StereoPanner | balance param | AudioMixer method | Pan property |
| Thread safety | JS single-thread | — | — | `lock (_sync)` |
| Key gotcha | Autoplay policy | 48 kHz fixed | No per-ID volume | Lock required |
