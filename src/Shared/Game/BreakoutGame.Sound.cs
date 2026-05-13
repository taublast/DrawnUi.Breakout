using Breakout.Helpers;
using System.Numerics;
using AppoMobi.Specials;

namespace Breakout.Game
{
    public partial class BreakoutGame : MauiGame
    {
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

        private async Task InitializeAudioAsync()
        {
#if !BROWSER
            IAudioService audioService;

#if !ANDROID
            audioService = new AudioMixerService(Plugin.Maui.Audio.AudioManager.Current);
#else
            audioService = new SoundFlowAudioService();
#endif

            // Preload
            await audioService.PreloadSoundAsync("oops", "Fx/ballout.mp3");
            await audioService.PreloadSoundAsync("collide", "Fx/bricksynth.wav");
            await audioService.PreloadSoundAsync("aggro", "Fx/powerup27.mp3");
            await audioService.PreloadSoundAsync("dlg", "Fx/quirky26.mp3");
            await audioService.PreloadSoundAsync("sel", "Fx/quirky7.mp3");
            await audioService.PreloadSoundAsync("joy", "Fx/synthchime2.mp3");
            await audioService.PreloadSoundAsync("sad", "Fx/bells1.mp3");

            await audioService.PreloadSoundAsync("brick", "Sounds/tik.wav");
            await audioService.PreloadSoundAsync("board2", "Sounds/bricksynth2.wav");
            await audioService.PreloadSoundAsync("board3", "Sounds/bricksynth3.wav");
            await audioService.PreloadSoundAsync("wall", "Sounds/brickglass.wav");
            await audioService.PreloadSoundAsync("start", "Sounds/gamestart.wav");
            await audioService.PreloadSoundAsync("ball", "Sounds/pong.wav");
            await audioService.PreloadSoundAsync("bip", "Sounds/bip.wav");
            await audioService.PreloadSoundAsync("bip1", "Sounds/bip1.wav");
            await audioService.PreloadSoundAsync("bip2", "Sounds/bip2.wav");

            await audioService.PreloadSoundAsync("one", "Sounds/one.wav");
            await audioService.PreloadSoundAsync("two", "Sounds/two.wav");

            await audioService.PreloadSoundAsync("demo", "Music/demoHypnoticPuzzle4.mp3");
            await audioService.PreloadSoundAsync("play", "Music/lvl1PixelCityGroovin.mp3");
            await audioService.PreloadSoundAsync("speedy", "Music/MonkeyDrama.mp3");
            await audioService.PreloadSoundAsync("tronic", "Music/TechnoTronic.mp3");

            _audioService = audioService;

            var soundsOn = AppSettings.Get(AppSettings.SoundsOn, AppSettings.SoundsOnDefault);
            EnableSounds(soundsOn);

            SetupBackgroundMusic();
#endif
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

        #endregion
    }
}
