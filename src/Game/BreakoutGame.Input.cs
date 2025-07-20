using AppoMobi.Maui.Gestures;
using AppoMobi.Specials;
using Breakout.Game.Dialogs;
using Breakout.Helpers;
using DrawnUi.Controls;
using System.Globalization;

namespace Breakout.Game
{
    public partial class BreakoutGame : MauiGame
    {
        #region GESTURES AND KEYS

        public void ApplyGameKey(GameKey gameKey)
        {
            // For playing state, set movement flags
            if (State == GameState.Playing || State == GameState.DemoPlay)
            {
                if (gameKey == GameKey.Stop)
                {
                    _moveLeft = false;
                    _moveRight = false;
                }
                else if (gameKey == GameKey.Left)
                {
                    _moveLeft = true;
                    _moveRight = false;
                }
                else if (gameKey == GameKey.Right)
                {
                    _moveLeft = false;
                    _moveRight = true;
                }

                if (gameKey == GameKey.Fire)
                {
                    if (!Ball.IsMoving)
                    {
                        //serve the ball!
                        Ball.IsMoving = true;
                    }
                    else if (Paddle.Powerup == PowerupType.Destroyer)
                    {
                        FirePaddleBullet();
                    }
                }
            }
        }

        // Keyboard handling: allow arrow keys and space to start/restart.
        // (Mappings can be customized as needed.)
        public override void OnKeyDown(MauiKey mauiKey)
        {
            var gameKey = MapToGame(mauiKey);

            if (State == GameState.Playing && gameKey == GameKey.Demo)
            {
                State = GameState.DemoPlay;
                //ToggleDemoMode();
                return;
            }

            if (State == GameState.DemoPlay &&
                (gameKey == GameKey.Left || gameKey == GameKey.Right || gameKey == GameKey.Fire))
            {
                State = GameState.Playing;
            }

            ApplyGameKey(gameKey);
        }

        void PauseGame()
        {
            if (State == GameState.Playing)
            {
                State = GameState.Paused;
                _moveLeft = false;
                _moveRight = false;
                GameDialog.Show(this, null, ResStrings.StatePaused.ToUpperInvariant(), null, () =>
                {
                    TogglePause();
                });
            }
        }

        bool TogglePause()
        {
            //if (State == GameState.Playing)
            //{
            //    ShowOptions();
            //    //PauseGame();
            //    return true;
            //}

            if (State == GameState.Paused)
            {
                State = PreviousState;
                _moveLeft = false;
                _moveRight = false;
                _ = GameDialog.PopAllAsync(this);
                return true;
            }
            else
            {
                ShowOptions();
                return true;
            }

            return false;
        }

        GameKey MapToGame(MauiKey key)
        {
            switch (key)
            {
                case MauiKey.Enter:
                case MauiKey.Space:
                    return GameKey.Fire;

                case MauiKey.KeyP:
                case MauiKey.Escape:
                case MauiKey.Pause:
                    return GameKey.Pause;

                case MauiKey.ArrowLeft:
                    return GameKey.Left;

                case MauiKey.ArrowRight:
                    return GameKey.Right;

                case MauiKey.KeyD:
                    return GameKey.Demo;
            }

            return GameKey.Unset;
        }

        public override void OnKeyUp(MauiKey mauiKey)
        {
            var gameKey = MapToGame(mauiKey);

            if (gameKey == GameKey.Pause)
            {
                if (TogglePause())
                    return;
            }


            if (mauiKey == MauiKey.KeyN)
            {
                StartNewLevel();
                return;
            }

            if (mauiKey == MauiKey.KeyR)
            {
                // Toggle collision detection system
                USE_RAYCAST_COLLISION = !USE_RAYCAST_COLLISION;
                System.Diagnostics.Debug.WriteLine(
                    $"Collision system switched to: {(USE_RAYCAST_COLLISION ? "RAYCAST" : "AABB")}");
                return;
            }

            if (State == GameState.Playing || State == GameState.DemoPlay)
            {
                if (gameKey == GameKey.Left)
                    _moveLeft = false;
                else if (gameKey == GameKey.Right)
                    _moveRight = false;
            }
        }

        /// <summary>
        /// Since touch gestures can come several for 1 frame we enqueue them and will process at start of the frame
        /// </summary>
        protected Queue<GameKey> GameKeysQueue = new();

        private GameState _lastStateChecked;
        private float thresholdTapVelocity = 200;


        public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args,
            GestureEventProcessingInfo apply)
        {
            ISkiaGestureListener consumed = null;

            ISkiaGestureListener PassToChildren()
            {
                return base.ProcessGestures(args, apply);
            }

            if (GameDialog.IsAnyDialogOpen(this))
            {
                return PassToChildren();
            }

            consumed = PassToChildren();
            if (consumed != null && consumed != this)
            {
                return consumed;
            }

            if (State == GameState.DemoPlay && args.Type == TouchActionResult.Down)
            {
                State = GameState.Playing;
            }

            if (State == GameState.Playing)
            {
                var velocityX = (float)(args.Event.Distance.Velocity.X / RenderingScale);

                if (args.Type == TouchActionResult.Panning)
                {
                    _wasPanning = true;

                    if (velocityX < 0)
                    {
                        _moveLeft = true;
                        _moveRight = false;
                    }
                    else if (velocityX > 0)
                    {
                        _moveRight = true;
                        _moveLeft = false;
                    }

                    return this;
                }
                else if (args.Type == TouchActionResult.Down)
                {
                    _wasPanning = false;
                    _isPressed = true;

                    if (args.Event.NumberOfTouches > 1) //lets say its fire
                    {
                        if (State == GameState.Playing)
                        {
                            GameKeysQueue.Enqueue(GameKey.Fire);
                        }
                    }
                }
                else if (args.Type == TouchActionResult.Tapped)
                {
                    if (State == GameState.Playing)
                    {
                        GameKeysQueue.Enqueue(GameKey.Fire);
                    }
                }
                else if (args.Type == TouchActionResult.Up)
                {
                    _isPressed = false;
                }

                _moveLeft = _moveRight = false;
                return this;
            }

            _moveLeft = _moveRight = false;
            return base.ProcessGestures(args, apply);
        }

        #endregion

        public class GridArrows : SkiaGrid
        {
            //base for future HUD in todo state
            //public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args, GestureEventProcessingInfo apply)
            //{
            //    return base.ProcessGestures(args, apply);
            //}

            private BreakoutGame _game;

            public GridArrows(BreakoutGame game)
            {
                _game = game;
            }

            public override void OnWillDisposeWithChildren()
            {
                base.OnWillDisposeWithChildren();

                _game = null;
            }
        }
    }
}