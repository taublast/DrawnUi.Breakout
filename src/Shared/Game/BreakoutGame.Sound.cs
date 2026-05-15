using Breakout.Helpers;
using System.Numerics;
using AppoMobi.Specials;

namespace Breakout.Game
{
    public partial class BreakoutGame : MauiGame
    {
        private static readonly (string SoundId, string FilePath)[] StartupAudioAssets =
        {
            ("oops", "Fx/ballout.mp3"),
            ("collide", "Fx/bricksynth.wav"),
            ("aggro", "Fx/powerup27.mp3"),
            ("dlg", "Fx/quirky26.mp3"),
            ("sel", "Fx/quirky7.mp3"),
            ("joy", "Fx/synthchime2.mp3"),
            ("sad", "Fx/bells1.mp3"),
            ("brick", "Sounds/tik.wav"),
            ("board2", "Sounds/bricksynth2.wav"),
            ("board3", "Sounds/bricksynth3.wav"),
            ("wall", "Sounds/brickglass.wav"),
            ("start", "Sounds/gamestart.wav"),
            ("ball", "Sounds/pong.wav"),
            ("bip", "Sounds/bip.wav"),
            ("bip1", "Sounds/bip1.wav"),
            ("bip2", "Sounds/bip2.wav"),
            ("one", "Sounds/one.wav"),
            ("two", "Sounds/two.wav"),
            ("demo", "Music/demoHypnoticPuzzle4.mp3"),
            ("play", "Music/lvl1PixelCityGroovin.mp3"),
            ("speedy", "Music/MonkeyDrama.mp3"),
            ("tronic", "Music/TechnoTronic.mp3")
        };

        #region AUDIO

        public enum Sound
        {
            None,
            Board,
            Brick,
            Wall,
            Oops,
            Start,
            PowerUp,
            PowerDown,
            Attack,
            Dialog,
            Selection,
            Joy,
            Sad
        }

        public void EnableSounds(bool state)
        {
            soundsOn = state;
        }

        private bool soundsOn;
        private bool _webAudioResumeFailureShown;

        private int GetAudioStartupAssetCount()
        {
            return StartupAudioAssets.Length;
        }

        private static IAudioService CreateAudioService()
        {
#if BROWSER
            return new WebAudioService();
#elif ANDROID
            return new SoundFlowAudioService();
#else
            return new AudioMixerService(Plugin.Maui.Audio.AudioManager.Current);
#endif
        }

        private async Task InitializeAudioAsync(Action<int, int, string>? reportProgress = null)
        {
            var audioService = CreateAudioService();
            var totalAssets = StartupAudioAssets.Length;

            reportProgress?.Invoke(0, totalAssets, ResStrings.LoadingAssets);

            for (int index = 0; index < StartupAudioAssets.Length; index++)
            {
                var asset = StartupAudioAssets[index];
                var loaded = await audioService.PreloadSoundAsync(asset.SoundId, asset.FilePath);
                if (!loaded)
                {
                    throw CreateAudioInitializationException(audioService, asset.SoundId, asset.FilePath);
                }

                reportProgress?.Invoke(index + 1, totalAssets, ResStrings.LoadingAssets);
            }

            _audioService = audioService;

            var soundsOn = AppSettings.Get(AppSettings.SoundsOn, AppSettings.SoundsOnDefault);
            EnableSounds(soundsOn);

            SetupBackgroundMusic();
        }

        private static Exception CreateAudioInitializationException(IAudioService audioService, string soundId,
            string filePath)
        {
            var message = $"Failed to load audio asset '{soundId}' from '{filePath}'.";

#if BROWSER
            if (audioService is WebAudioService webAudio && !string.IsNullOrWhiteSpace(webAudio.LastErrorMessage))
            {
                message = $"{message} {webAudio.LastErrorMessage}";
            }
#endif

            return new InvalidOperationException(message);
        }


        public void PlaySound(Sound sound, System.Numerics.Vector3 position = default)
        {
            if (_audioService == null || !soundsOn)
                return;

            if (State == GameState.DemoPlay && !USE_SOUND_IN_DEMO)
            {
                return;
            }

            Tasks.StartDelayedAsync(TimeSpan.FromMicroseconds(1), async () =>
            {
                if (position == default)
                {
                    position = new Vector3(01f, 1, 1f);
                }

                switch (sound)
                {
                case Sound.Board:
                _audioService.PlaySpatialSound("ball", position, 0.5f);
                break;
                case Sound.Brick:
                _audioService.PlaySpatialSound("board2", position);
                break;
                case Sound.Wall:
                _audioService.PlaySpatialSound("board3", position, 0.5f);
                break;
                case Sound.Oops:
                _audioService.PlaySound("oops", 0.75f);
                break;
                case Sound.Joy:
                _audioService.PlaySound("joy", 0.95f);
                break;
                case Sound.Sad:
                _audioService.PlaySound("sad", 0.95f);
                break;
                case Sound.Dialog:
                _audioService.PlaySound("dlg", 0.75f);
                break;
                case Sound.Selection:
                _audioService.PlaySound("sel", 0.75f);
                break;
                case Sound.Start:
                _audioService.PlaySound("start", 0.75f);
                break;
                case Sound.PowerUp:
                break;
                case Sound.PowerDown:
                break;
                case Sound.Attack:
                _audioService.PlaySound("aggro", 0.66f);
                break;
                }
            });
        }


        public void PlaySpecialMusic()
        {
            var musicOn = AppSettings.Get(AppSettings.MusicOn, AppSettings.MusicOnDefault);

            if (musicOn && State == GameState.Playing)
            {
                _audioService?.StartBackgroundMusic("tronic", 1.0f);
            }
        }

        public void PlaySpeedyMusic()
        {
            var musicOn = AppSettings.Get(AppSettings.MusicOn, AppSettings.MusicOnDefault);

            if (musicOn && State == GameState.Playing)
            {
                _audioService?.StartBackgroundMusic("speedy", 1.0f);
            }
        }

        public void SetupBackgroundMusic()
        {
            var musicOn = AppSettings.Get(AppSettings.MusicOn, AppSettings.MusicOnDefault);
            SetupBackgroundMusic(musicOn);
        }

        protected void SetupBackgroundMusic(bool isOn)
        {
            if (!isOn)
            {
                StopBackgroundMusic();
            }
            else
            {
                if (State == GameState.Playing)
                {
                    PlayMusicLooped(Level);
                }
                else
                {
                    PlayMusicLooped(0);
                }
            }
        }

        public void ToggleSound()
        {
            if (_audioService == null)
            {
                return;
            }

            _audioService.IsMuted = !_audioService.IsMuted;
        }

        public void PlayMusicLooped(int lvl)
        {
            if (_audioService == null || !AppSettings.Get(AppSettings.MusicOn, AppSettings.MusicOnDefault))
            {
                return;
            }

            if (lvl > 0)
            {
                _audioService?.StartBackgroundMusic("play", 1.0f);
            }
            else
            {
                _audioService?.StartBackgroundMusic("demo", 1.0f);
            }
        }

        public void StopBackgroundMusic()
        {
            _audioService?.StopBackgroundMusic();
        }

        public void SetBackgroundMusicVolume(float volume)
        {
            _audioService?.SetSoundVolume("background", volume);
        }

        public bool IsBackgroundMusicPlaying => _audioService?.IsBackgroundMusicPlaying == true;

        private void NotifyAudioUserGesture()
        {
#if BROWSER
            if (_audioService is WebAudioService webAudio)
            {
                _ = EnsureWebAudioRunningAfterGestureAsync(webAudio);
            }
#endif
        }

#if BROWSER
        private async Task EnsureWebAudioRunningAfterGestureAsync(WebAudioService webAudio)
        {
            if (!AppSettings.Get(AppSettings.MusicOn, AppSettings.MusicOnDefault)
                && !AppSettings.Get(AppSettings.SoundsOn, AppSettings.SoundsOnDefault))
            {
                return;
            }

            if (await webAudio.TryResumeAfterUserGestureAsync())
            {
                return;
            }

            if (_webAudioResumeFailureShown)
            {
                return;
            }

            _webAudioResumeFailureShown = true;
            ShowStartupAssetFailureDialog(new StartupAssetFailure(
                "Audio could not start.",
                "The game will continue without sound.",
                webAudio.LastErrorMessage));
        }
#endif

        #endregion
    }
}
