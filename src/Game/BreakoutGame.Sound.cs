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
            await audioService.PreloadSoundAsync("aggro", "Fx/powerup27.mp3");
            await audioService.PreloadSoundAsync("dlg", "Fx/quirky26.mp3");
            await audioService.PreloadSoundAsync("sel", "Fx/quirky7.mp3");
            await audioService.PreloadSoundAsync("joy", "Fx/synthchime2.mp3");
            await audioService.PreloadSoundAsync("sad", "Fx/bells1.mp3");
            //todo maybe
            //await audioService.PreloadSoundAsync("powerdown", "Fx/????.mp3");
            //await audioService.PreloadSoundAsync("powerup", "Fx/????.mp3");

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

            // Background music files will be streamed directly for memory efficiency
            // Both SoundFlow and AudioMixer now support streaming from files

            // Preload background music
            //we need to preload as Soundflow actually has a problem to get valid Length when reading from mobile file
            //so in the future we could play from file when its fixed
            await audioService.PreloadSoundAsync("demo", "Music/demoHypnoticPuzzle4.mp3");
            await audioService.PreloadSoundAsync("play", "Music/lvl1PixelCityGroovin.mp3");
            await audioService.PreloadSoundAsync("speedy", "Music/MonkeyDrama.mp3");

            _audioService = audioService;

            var soundsOn = AppSettings.Get(AppSettings.SoundsOn, AppSettings.SoundsOnDefault);
            EnableSounds(soundsOn);

            SetupBackgroundMusic();
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
                //_audioService.PlaySound("powerup", 0.5f);
                break;
                case Sound.PowerDown:
                //_audioService.PlaySound("powerdown", 0.33f);
                break;
                case Sound.Attack:
                _audioService.PlaySound("aggro", 0.66f);
                break;
                }
            });
        }


        public void PlaySpeedyMusic()
        {
            if (State == GameState.Playing)
            {
                //_audioService.StartBackgroundMusicFromFile("Music/MonkeyDrama.mp3", 1.0f);
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

        /// <summary>
        /// Starts the background music loop using efficient file streaming
        /// </summary>
        public void PlayMusicLooped(int lvl)
        {
            if (_audioService == null || !AppSettings.Get(AppSettings.MusicOn, AppSettings.MusicOnDefault))
            {
                return;
            }

            // Stream background music directly from files (memory efficient)
            if (lvl > 0)
            {
                //_audioService.StartBackgroundMusicFromFile("Music/lvl1PixelCityGroovin.mp3", 1.0f);
                _audioService?.StartBackgroundMusic("play", 1.0f);
            }
            else
            {
                //_audioService.StartBackgroundMusicFromFile("Music/demoHypnoticPuzzle4.mp3", 0.5f);
                _audioService?.StartBackgroundMusic("demo", 1.0f);
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