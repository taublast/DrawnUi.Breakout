using AppoMobi.Maui.Gestures;
using Breakout.Game.Dialogs;
using Breakout.Game.Input;
using Breakout.Helpers;

namespace Breakout.Game
{
    public partial class BreakoutGame : MauiGame
    {
        #region GESTURES AND KEYS

        protected void InitializeInput()
        {
            SetInputPressMode(AppSettings.Get(AppSettings.InputPressEnabled, AppSettings.InputPressEnabledDefault));

            //GAME CONTROLLER SUPPORT
            AddInputController(new GameControllerInput(this));
        }

        public virtual void SendKey(GameKey key)
        {
            GameKeysQueue.Enqueue(key);
        }

        private int ActionDelayMs = 2000;
 
        private Dictionary<GameKey, long> LastActions = new();

        private bool CanProcessSpecialKey(GameKey key)
        {
            var currentTime = Environment.TickCount64;
            var allowed = true;
            if (LastActions.TryGetValue(key, out var lastActionTime))
            {
                allowed = currentTime - lastActionTime >= ActionDelayMs;
            }
            return allowed;
        }

        private void MarkCanProcessSpecialKey(GameKey key)
        {
            LastActions[key] = Environment.TickCount64;
        }

        public void ProcessInput()
        {
            foreach (var inputController in InputControllers)
            {
                inputController.ProcessState();
            }

            while (GameKeysQueue.Count > 0)
            {
                var gameKey = GameKeysQueue.Dequeue();

                //special global keys
                if (gameKey == GameKey.Pause)
                {
                    if (!CanProcessSpecialKey(GameKey.Pause))
                    {
                        continue;
                    }
                    MarkCanProcessSpecialKey(gameKey); 
                }
                else
                //keys that can go to dialogs too
                if (GameDialog.IsAnyDialogOpen(this))
                {
                    var topDialog = GameDialog.GetTopDialog(this);
                    if (topDialog is IGameKeyHandler handler && handler.HandleGameKey(gameKey))
                    {
                        continue;
                    }
                }

                // If dialog didn't consume apply to game
                ApplyGameKey(gameKey);
            }
        }

        protected List<IInputController> InputControllers = new();

        public void AddInputController(IInputController controller)
        {
            InputControllers.Add(controller);
        }

        public void RemoveInputController(IInputController controller)
        {
            InputControllers.Remove(controller);
        }

        /// <summary>
        /// Since touch gestures can come several for 1 frame we enqueue them and will process at start of the frame
        /// </summary>
        protected Queue<GameKey> GameKeysQueue = new();

        private GameState _lastStateChecked;
        private float thresholdTapVelocity = 200;


        /// <summary>
        /// You could use this to simulate key presses from game controller/anything
        /// </summary>
        /// <param name="gameKey"></param>
        public virtual void ApplyGameKey(GameKey gameKey)
        {

            if (gameKey == GameKey.Pause)
            {
                TogglePause();
                return;
            }

            // For playing state, set movement flags
            if (State == GameState.Playing || State == GameState.DemoPlay)
            {
                if (gameKey == GameKey.Fire)
                {
                    // Check if any balls are not moving (sticky)
                    bool hasStickyballs = ActiveBalls.Any(ball => !ball.IsMoving);

                    if (hasStickyballs)
                    {
                        // Release ALL sticky balls
                        foreach (var ball in ActiveBalls)
                        {
                            if (!ball.IsMoving)
                            {
                                ball.IsMoving = true;
                            }
                        }
                    }
                    else if (Paddle.Powerup == PowerupType.Destroyer)
                    {
                        FirePaddleBullet();
                    }
                }

                if (gameKey == GameKey.Stop)
                {
                    IsMovingLeft = false;
                    IsMovingRight = false;
                }
                else if (gameKey == GameKey.Left)
                {
                    IsMovingLeft = true;
                    IsMovingRight = false;
                }
                else if (gameKey == GameKey.Right)
                {
                    IsMovingLeft = false;
                    IsMovingRight = true;
                }


            }
        }


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

            if (State == GameState.Playing)
            {
                var velocityX = (float)(args.Event.Distance.Velocity.X / RenderingScale);

                if (!InputPressMode)
                {
                    if (args.Type == TouchActionResult.Panning)
                    {
                        WasPanning = true;

                        if (velocityX < 0)
                        {
                            IsMovingLeft = true;
                            IsMovingRight = false;
                        }
                        else if (velocityX > 0)
                        {
                            IsMovingRight = true;
                            IsMovingLeft = false;
                        }

                        return this;
                    }
                }

                if (args.Type == TouchActionResult.Down)
                {
                    WasPanning = false;
                    IsPressed = true;

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
                    IsPressed = false;
                }

                IsMovingLeft = IsMovingRight = false;
                return this;
            }

            IsMovingLeft = IsMovingRight = false;
            return base.ProcessGestures(args, apply);
        }

        void PauseGame()
        {
            if (State == GameState.Playing)
            {
                State = GameState.Paused;
                IsMovingLeft = false;
                IsMovingRight = false;
                GameDialog.Show(this, null, ResStrings.StatePaused.ToUpperInvariant(), null, () => { TogglePause(); });
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
                IsMovingLeft = false;
                IsMovingRight = false;
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

                case MauiKey.ArrowDown:
                    return GameKey.Down;

                case MauiKey.ArrowUp:
                    return GameKey.Up;

                case MauiKey.KeyD:
                    return GameKey.Demo;
            }

            return GameKey.Unset;
        }

        /*
        /// <summary>
        /// Maps a GameKey to its primary MauiKey equivalent
        /// </summary>
        /// <param name="gameKey">The game key to map</param>
        /// <returns>The corresponding MauiKey</returns>
        public MauiKey MapToKeyboard(GameKey gameKey)
        {
            switch (gameKey)
            {
                case GameKey.Fire:
                    return MauiKey.Space;
                case GameKey.Pause:
                    return MauiKey.KeyP;
                case GameKey.Left:
                    return MauiKey.ArrowLeft;
                case GameKey.Right:
                    return MauiKey.ArrowRight;
                case GameKey.Demo:
                    return MauiKey.KeyD;
                case GameKey.Unset:
                default:
                    return MauiKey.Unknown;
            }
        }
        */

        protected void SetInputPressMode(bool state)
        {
            AppSettings.Set(AppSettings.InputPressEnabled, state);
            InputPressMode = state;
        }

        public bool InputPressMode { get; protected set; }

        #endregion
    }
}