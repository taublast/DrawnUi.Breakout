using System.Diagnostics;
using AppoMobi.Maui.Gestures;
using AppoMobi.Specials;
using SkiaSharp;
using System.Numerics;
using System.Runtime.CompilerServices;
using DrawnUi.Draw;
using BreakoutGame.Game.Dialogs;
using BreakoutGame.Game.Ai;

namespace BreakoutGame.Game
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
            Start
        }

        public void PlaySound(Sound sound, System.Numerics.Vector3 position = default)
        {
            if (_audioService == null)
                return;

            if (position == default)
            {
                position = new Vector3(01f, 1, 1f);
            }

            switch (sound)
            {
                case Sound.Board:
                    if (State != GameState.DemoPlay)
                        _audioService.PlaySpatialSound("ball", position, 0.5f);
                    break;
                case Sound.Brick:
                    if (State != GameState.DemoPlay)
                        _audioService.PlaySpatialSound("board2", position);
                    break;
                case Sound.Wall:
                    if (State != GameState.DemoPlay)
                        _audioService.PlaySpatialSound("board3", position, 0.5f);
                    break;
                case Sound.Oops:
                    if (State != GameState.DemoPlay)
                        _audioService.PlaySound("oops", 0.75f);
                    break;
                case Sound.Start:
                    _audioService.PlaySound("start", 0.75f);
                    break;
            }
        }

        private async Task InitializeAudioAsync()
        {
            var audioService = new AudioMixerService(Plugin.Maui.Audio.AudioManager.Current);

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

            StartBackgroundMusic(0);
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
            if (_audioService == null)
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