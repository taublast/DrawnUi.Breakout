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
            Powerup
        }

        public void EnableSounds(bool state)
        {
            soundsOn = state;
        }

        private bool soundsOn;

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
                    case Sound.Start:
                        _audioService.PlaySound("start", 0.75f);
                        break;
                    case Sound.Powerup:
                        _audioService.PlaySound("powerup", 0.6f);
                        break;
                }
            });
        }

        private async Task InitializeAudioAsync()
        {
            IAudioService audioService;

#if WINDOWS
            audioService = new AudioMixerService(Plugin.Maui.Audio.AudioManager.Current);
#else
            audioService = new SoundFlowAudioService();
#endif

            // Preload
            //will keep
            await audioService.PreloadSoundAsync("oops", "Fx/ballout.mp3");
            await audioService.PreloadSoundAsync("collide", "Fx/bricksynth.wav");

            //maybe
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

            // Preload background music

            await audioService.PreloadSoundAsync("demo", "Music/demoHypnoticPuzzle4.mp3");
            await audioService.PreloadSoundAsync("play", "Music/lvl1PixelCityGroovin.mp3");

            _audioService = audioService;

            var soundsOn = AppSettings.Get(AppSettings.SoundsOn, AppSettings.SoundsOnDefault);
            EnableSounds(soundsOn);

            var musicOn = AppSettings.Get(AppSettings.MusicOn, AppSettings.MusicOnDefault);
            SetupBackgroundMusic(musicOn);
        }

        public void SetupBackgroundMusic(bool isOn)
        {
            if (!isOn)
            {
                StopBackgroundMusic();
            }
            else
            {
                if (State == GameState.Playing)
                {
                    StartBackgroundMusic(Level);
                }
                else
                {
                    StartBackgroundMusic(0);
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

        /// <summary>
        /// Starts the background music loop
        /// </summary>
        public void StartBackgroundMusic(int lvl)
        {
            if (_audioService == null || !AppSettings.Get(AppSettings.MusicOn, AppSettings.MusicOnDefault))
            {
                return;
            }

            _audioService.StopBackgroundMusic();
            if (lvl > 0)
            {
                _audioService?.StartBackgroundMusic("play", 0.5f);
            }
            else
            {
                _audioService?.StartBackgroundMusic("demo", 0.5f);
            }
        }

        /// <summary>
        /// Stops the background music
        /// </summary>
        public void StopBackgroundMusic()
        {
            _audioService?.StopBackgroundMusic();
        }

        /// <summary>
        /// Sets the background music volume
        /// </summary>
        /// <param name="volume">Volume level (0.0 to 1.0)</param>
        public void SetBackgroundMusicVolume(float volume)
        {
            _audioService?.SetSoundVolume("background", volume);
        }

        /// <summary>
        /// Gets whether background music is currently playing
        /// </summary>
        public bool IsBackgroundMusicPlaying => _audioService?.IsBackgroundMusicPlaying == true;

        #endregion
    }
}