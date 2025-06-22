using System.Diagnostics;
using AppoMobi.Maui.Gestures;
using AppoMobi.Specials;
using SkiaSharp;
using System.Numerics;
using System.Runtime.CompilerServices;
using DrawnUi.Draw;

namespace BreakoutGame.Game
{
    public partial class BreakoutGame : MauiGame
    {
        #region CONSTANTS

        public const float BALL_SPEED = 375f;
        public const float PADDLE_SPEED = 550;
        public const float PADDLE_WIDTH = 90;
        public const int MAX_BRICKS = 100;
        public const int MAX_BRICKS_COLUMNS = 9;
        public const int MIN_BRICKS_ROWS = 3;
        public const float SPACING_BRICKS = 4f;
        public const float BRICKS_SIDE_MARGIN = 16f;
        public const float BRICKS_TOP_MARGIN = 110f;
        public const int LIVES = 3;

        /// <summary>
        /// For long running profiling
        /// </summary>
        const bool CHEAT_INVULNERABLE = true;

        /// <summary>
        /// Compile-time flag to enable raycasting collision detection instead of AABB intersection
        /// Set to true to use raycasting, false to use traditional AABB collision detection
        /// </summary>
        public static bool USE_RAYCAST_COLLISION = true;

        #endregion

        #region INITIALIZE

        private AIPaddleController _aiController;
        public AIPaddleController AIController => _aiController ??= new AIPaddleController(this, AIDifficulty.Medium);

        public GameAudioService? _audioService;
        public BallSprite Ball;
        public PaddleSprite Paddle;
        private SkiaLabel LabelScore;

        public BreakoutGame()
        {
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
            BackgroundColor = Colors.DarkSlateBlue;

            Children = new List<SkiaControl>()
            {
                new BallSprite()
                {
                    ZIndex = 4
                }.Assign(out Ball),

                new PaddleSprite()
                {
                    ZIndex = 5,
                    Top = -28
                }.Assign(out Paddle),

                new SkiaLabel()
                {
                    ZIndex = 110,
                    UseCache = SkiaCacheType.GPU,
                    Margin = 16,
                    FontFamily = "FontGame",
                    FontSize = 19,
                    StrokeColor = AmstradColors.DarkBlue,
                    TextColor = AmstradColors.White,
                    DropShadowColor = Colors.DarkBlue,
                    DropShadowOffsetX = 2,
                    DropShadowOffsetY = 2,
                    DropShadowSize = 2,
                    FillGradient = new()
                    {
                        Colors = new List<Color>()
                        {
                            Colors.White,
                            Colors.CornflowerBlue
                        }
                    }
                }.Assign(out LabelScore)
            };

            BindingContext = this;

            Instance = this;

            //_ = InitializeAudioAsync();

            _aiController = new AIPaddleController(this, AIDifficulty.Hard);

            //BackgroundColor = AmstradColors.DarkBlue;

            //FrameTimeInterpolator.TargetFps = 35;
        }

        #endregion

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
                    _audioService.PlaySpatialSound("board", position, 1f);
                    break;
                case Sound.Brick:
                    //_audioService.PlaySound("brick", 0.66f);
                    _audioService.PlaySound("brick", 1.2f);
            break;
                case Sound.Wall:
            //_audioService.PlaySpatialSound("board", position, 0.5f);
            _audioService.PlaySpatialSound("board", position, 0.75f);
            break;
                case Sound.Oops:
                    _audioService.PlaySound("oops", 1.0f);

                    //Debug.WriteLine("**********************************");
                    //Debug.WriteLine($"SOUNDS {_audioService.GetActivePlayerCount()}");
                    //Debug.WriteLine("**********************************");

                    break;
                case Sound.Start:
                    _audioService.PlaySound("start", 0.75f);
                    break;
            }
        }

        private async Task InitializeAudioAsync()
        {
            var audioService = new GameAudioService(Plugin.Maui.Audio.AudioManager.Current);

            // Preload all game sounds
            await audioService.PreloadSoundAsync("brick", "Sounds/tik.wav");
            await audioService.PreloadSoundAsync("board", "Sounds/bricksynth.wav");
            await audioService.PreloadSoundAsync("board2", "Sounds/bricksynth2.wav");
            await audioService.PreloadSoundAsync("board3", "Sounds/bricksynth3.wav");
            await audioService.PreloadSoundAsync("wall", "Sounds/brickglass.wav");
            await audioService.PreloadSoundAsync("start", "Sounds/gamestart.wav");
            await audioService.PreloadSoundAsync("oops", "Sounds/kill.wav");
            await audioService.PreloadSoundAsync("ball", "Sounds/pong.wav");
            await audioService.PreloadSoundAsync("bip", "Sounds/bip.wav");
            await audioService.PreloadSoundAsync("bip1", "Sounds/bip1.wav");
            await audioService.PreloadSoundAsync("bip2", "Sounds/bip2.wav");

            await audioService.PreloadSoundAsync("one", "Sounds/one.wav");
            await audioService.PreloadSoundAsync("two", "Sounds/two.wav");

            _audioService = audioService;

            // Start background music
            //_audioService.PlaySound("background", 0.5f, 0f, true);
        }

        public void ToggleSound()
        {
            _audioService.IsMuted = !_audioService.IsMuted;
        }

        #endregion

        /// <summary>
        /// So it can get paused/resumed from anywhere in the app
        /// </summary>
        public static BreakoutGame Instance { get; set; }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            BindingContext = this; //insist in case parent view might set its own
        }

        protected override void OnLayoutReady()
        {
            base.OnLayoutReady();

            Task.Run(async () =>
            {
                while (Superview == null || !Superview.HasHandler)
                {
                    await Task.Delay(30);
                }

                //we have some GPU cache used so we need the canvas to be fully created before we would start
                Initialize(); //game loop will be started inside
            }).ConfigureAwait(false);
        }

        bool _appeared;

        public override void OnAppeared()
        {
            base.OnAppeared();

            Tasks.StartDelayed(TimeSpan.FromSeconds(1), () =>
            {
                //our dialog has a transparent background that is blurring pixels under
                //so we have 2 options:
                //A - do not cache the backdrop and blur underground pixels in realtime
                //B - cache the backdrop but blur after the content below was rendered, so we implement the hack below, knowing our blurred background will be static as we show it only during pauses.

                _appeared = true;
                //OnPropertyChanged(nameof(ShowDialog));
            });
        }

        void Initialize()
        {
            if (!Superview.HasHandler || _initialized)
                return;

            RndExtensions.RandomizeTime(); //amstrad cpc forever

            IgnoreChildrenInvalidations = true;

            // in case we implement key press 
            Focus();

            _initialized = true;

            // Pool bricks for reuse
            for (int i = 0; i < MAX_BRICKS; i++)
            {
                AddToPoolBrickSprite();
            }


            // Set initial timestamp
            //LastFrameTimeNanos = SkiaControl.GetNanoseconds();
            //ResetGame();

            _needPrerender = true;

            _initialized = true;

            PresentGame();
        }

        protected override void Draw(DrawingContext context)
        {
            base.Draw(context);

            if (_needPrerender)
            {
                //prerender or precompile something like shaders etc
                // ...

                _needPrerender = false;

                //Tasks.StartDelayed(TimeSpan.FromSeconds(1), () =>
                //{
                //    StartNewGameDemo();
                //});
            }

            //if (Ball != null)
            //{
            //    Trace.WriteLine($"-------------");
            //    Trace.WriteLine($"BALL {Ball.GetPositionOnCanvas()}");
            //    Trace.WriteLine($"BALL {Ball.RenderedNode.HitBoxWithTransforms.Pixels.Location}");
            //    Trace.WriteLine($"-------------");
            //    //Trace.WriteLine($"PADDLE {Paddle.GetPositionOnCanvas()}");
            //}
        }

        #region ACTIONS

        private void AddBrick(string presetId, int col, int row,
            float brickWidth, float brickHeight, float margin, float offsetX, float offsetY)
        {
            var preset = BrickPresets.Presets[presetId];
            if (BricksPool.Count > 0)
            {
                var brick = BricksPool.Values.FirstOrDefault();
                if (brick != null && BricksPool.Remove(brick.Uid))
                {
                    brick.IsActive = true;
                    brick.WidthRequest = brickWidth;
                    brick.HeightRequest = brickHeight;
                    float xPos = margin + col * (brickWidth + margin);
                    float yPos = margin + row * (brickHeight + margin);
                    brick.Left = offsetX + xPos;
                    brick.Top = yPos + offsetY;

                    brick.BackgroundColor = preset.BackgroundColor;
                    brick.SupplementaryHitsToDestroy = preset.SupplementaryHitsToDestroy;
                    brick.Undestructible = preset.Undestructible;

                    brick.ResetAnimationState();
                    _spritesToBeAdded.Add(brick);
                }
            }
        }

        private void AddBrick(int col, int row, float brickWidth, float brickHeight, float margin)
        {
            if (BricksPool.Count > 0)
            {
                var brick = BricksPool.Values.FirstOrDefault();
                if (brick != null && BricksPool.Remove(brick.Uid))
                {
                    brick.IsActive = true;
                    brick.WidthRequest = brickWidth;
                    brick.HeightRequest = brickHeight;
                    float xPos = margin + col * (brickWidth + margin);
                    float yPos = margin + row * (brickHeight + margin);
                    brick.Left = xPos;
                    brick.Top = yPos;
                    // Color by row.
                    switch (row % 5)
                    {
                        case 0: brick.BackgroundColor = Colors.Red; break;
                        case 1: brick.BackgroundColor = Colors.Orange; break;
                        case 2: brick.BackgroundColor = Colors.Yellow; break;
                        case 3: brick.BackgroundColor = Colors.Green; break;
                        case 4: brick.BackgroundColor = Colors.Blue; break;
                    }

                    brick.ResetAnimationState();
                    _spritesToBeAdded.Add(brick);
                }
            }
        }

        public LevelManager LevelManager { get; set; }
        public int BricksLeft { get; set; }

        void LevelComplete()
        {
            // Store current game state before changing levels
            // Note: State is already GameState.LevelComplete at this point, so check PreviousState
            var wasInDemoMode = PreviousState == GameState.DemoPlay;

            // Stop the game loop
            StopLoop();

            if (wasInDemoMode)
            {
                // In demo mode, auto-continue to next level without showing dialog
                if (Level < 5)
                {
                    Level++;
                }
                else
                {
                    Level = 1; // Loop back to level 1 in demo mode
                }

                // Auto-continue demo mode
                StartNewLevel();
                State = GameState.DemoPlay;
                StartLoop();
            }
            else
            {
                // Normal mode - show level complete dialog
                if (Level < 5)
                {
                    Level++;
                    ShowLevelCompleteDialog();
                }
                else
                {
                    Level = 1;
                    ShowLevelCompleteDialog();
                }
            }
        }

        void StartNewLevel()
        {
            _levelCompletionPending = 0;

            lock (_lockSpritesToBeRemovedLater)
            {
                foreach (var control in Views)
                {
                    if (control is BrickSprite)
                    {
                        _spritesToBeRemovedLater.Enqueue(control);
                    }
                }
            }

            ProcessSpritesToBeRemoved();

            ResetBall();

            Ball.IsMoving = false;

            Ball.SpeedRatio = 1 + 0.2f * (Level - 1);

            // Set formation and presets based on level
            FormationType formation;
            List<string> allowedPresets = null;

            switch (Level)
            {
                case 1:
                    // Level 1: Simple grid with basic bricks
                    formation = FormationType.Grid;
                    allowedPresets = new List<string>
                    {
                        "Standard_Red",
                        "Standard_Blue",
                        "Standard_Green",
                        "Standard_Orange",
                        "Standard_Yellow"
                    };
                    break;

                case 2:
                    // Level 2: Arch formation with some reinforced bricks
                    formation = FormationType.Arch;
                    allowedPresets = new List<string>
                    {
                        "Standard_Red",
                        "Standard_Blue",
                        "Standard_Green",
                        "Standard_Orange",
                        "Standard_Yellow",
                        "Reinforced_Brown"
                    };
                    break;

                case 3:
                    // Level 3: Diamond formation with all brick types
                    formation = FormationType.Diamond;
                    // null means use all available presets
                    break;

                case 4:
                    // Level 4: Pyramid with tough bricks
                    formation = FormationType.Pyramid;
                    allowedPresets = new List<string>
                    {
                        "Standard_Red", "Standard_Blue", "Reinforced_Brown", "Hard_DarkGray"
                    };
                    break;

                case 5:
                    // Level 5: Wave pattern with special bricks
                    formation = FormationType.Wave;
                    // null means use all available presets
                    break;

                default:
                    // For higher levels, use more complex and varied patterns
                    // Use modulo to cycle through formations
                    int formationIndex = (Level - 6) % 8;
                    formation = (FormationType)formationIndex;
                    // null means use all available presets
                    break;
            }

            // Generate the level
            var brickPositions = LevelManager.GenerateLevel(
                Level,
                (float)Width - BRICKS_SIDE_MARGIN * 2,
                (float)Height / 2,
                formation,
                allowedPresets
            );

            BricksLeft = LevelManager.CountBreakableBricks(brickPositions);

            // Calculate brick dimensions based on columns and rows
            int columns = brickPositions.Max(p => (int)p.Column) + 1;
            float margin = SPACING_BRICKS;
            float totalSpacing = margin * (columns + 1);
            float availableWidth = (float)Width - totalSpacing - BRICKS_SIDE_MARGIN * 2;
            float brickWidth = availableWidth / columns;
            float brickHeight = 20f; // Fixed brick height as in original code

            float offsetBricksY = BRICKS_TOP_MARGIN;

            // Add bricks using the existing pool and AddBrick method
            foreach (var position in brickPositions)
            {
                // Use the row and column from position
                int col = (int)position.Column;
                int row = (int)position.Row;
                string presetId = position.PresetId;

                // Skip if no preset was assigned
                if (string.IsNullOrEmpty(presetId))
                    continue;


                // Use existing AddBrick method that handles pooling
                AddBrick(presetId, col, row, brickWidth, brickHeight, margin, BRICKS_SIDE_MARGIN, offsetBricksY);

                // Get the brick we just added (it should be the last one in _spritesToBeAdded)
                if (_spritesToBeAdded.Count > 0 && _spritesToBeAdded[_spritesToBeAdded.Count - 1] is BrickSprite brick)
                {
                    // Apply the preset to this brick
                    BrickPresets.ApplyPreset(brick, presetId);
                }
            }

            levelReady = false;

            // Preserve demo state if we're in demo mode, otherwise set to Playing
            if (State != GameState.DemoPlay)
            {
                State = GameState.Playing;
            }

            if (State == GameState.DemoPlay)
            {
                AIController.ResetTimers();
            }
        }

        private bool levelReady;

        public void StartNewGameDemo()
        {
            StartNewGame();
            State = GameState.DemoPlay;
        }

        public void StartNewGamePlayer()
        {
            StartNewGame();
            State = GameState.Playing;
        }


        void StartNewGame()
        {
            Score = 0;

            Lives = LIVES;

            Level = 1;

            LevelManager = new LevelManager();

            StartNewLevel();

            //ShowDialog = false;
            StartLoop();
        }

        void PresentGame()
        {
            // Start demo mode - bot plays behind the welcome dialog
            StartDemoMode();

            // Show welcome dialog
            GameDialog.Show(this, UiElements.DialogPrompt("Welcome to Breakout!\nUse mouse or touch to move the paddle. Break all the bricks to win!"), "START GAME", onOk: () =>
            {
                StartNewGamePlayer();
            });
        }

        void StartDemoMode()
        {
            // Initialize demo mode
            Score = 0;
            Lives = LIVES;
            Level = 1;
            LevelManager = new LevelManager();

            // Set demo state and start level
            State = GameState.DemoPlay;
            StartNewLevel();
            StartLoop();
        }

        void RestartDemoMode()
        {
            // Restart demo mode from level 1 without showing any dialogs
            Score = 0;
            Lives = LIVES;
            Level = 1;

            // Clear all bricks
            lock (_lockSpritesToBeRemovedLater)
            {
                foreach (var control in Views)
                {
                    if (control is BrickSprite)
                    {
                        _spritesToBeRemovedLater.Enqueue(control);
                    }
                }
            }
            ProcessSpritesToBeRemoved();

            // Reset ball and continue demo
            ResetBall();
            Ball.IsMoving = false;

            // Set demo state before starting new level
            State = GameState.DemoPlay;

            // Start new level in demo mode
            StartNewLevel();
            Update();
        }

        void ShowGameOverDialog()
        {
            // Show game over dialog
            var gameOverContent = new SkiaLabel()
            {
                Text = $"Game Over!\nFinal Score: {Score}\nBetter luck next time!",
                TextColor = Colors.White,
                FontSize = 16,
                HorizontalTextAlignment = DrawTextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
            };

            GameDialog.Show(this, gameOverContent, "PLAY AGAIN", "QUIT",
                onOk: () => ResetGame(),
                onCancel: () => {
                    // Could navigate back or close the game
                });
        }

        void ResetGame()
        {
            // Reset game state
            Lives = LIVES;
            Score = 0;
            Level = 1;
            State = GameState.Ready;

            // Clear all bricks
            lock (_lockSpritesToBeRemovedLater)
            {
                foreach (var control in Views)
                {
                    if (control is BrickSprite)
                    {
                        _spritesToBeRemovedLater.Enqueue(control);
                    }
                }
            }
            ProcessSpritesToBeRemoved();

            // Reset ball
            ResetBall();
            Ball.IsMoving = false;

            // Start new level
            StartNewLevel();
            Update();
        }

        async void ShowLevelCompleteDialog()
        {
            // Show level complete dialog
            var levelCompleteContent = new SkiaLabel()
            {
                Text = $"Level {Level - 1} Complete!\nScore: {Score}\nGet ready for Level {Level}!",
                TextColor = Colors.White,
                FontSize = 16,
                HorizontalTextAlignment = DrawTextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Center
            };

            if (await GameDialog.ShowAsync(this, levelCompleteContent, "CONTINUE")) 
            {
                // Start the new level
                StartNewLevel();
                State = GameState.Playing;
                StartLoop();
            }
        }

        // Example of using the async dialog method
        async void ShowExampleAsyncDialog()
        {
            var content = new SkiaLabel()
            {
                Text = "Do you want to continue?",
                TextColor = Colors.White,
                FontSize = 16,
                HorizontalTextAlignment = DrawTextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
            };

            bool result = await GameDialog.ShowAsync(this, content, "YES", "NO");

            if (result)
            {
                // User clicked YES
                // Do something...
            }
            else
            {
                // User clicked NO
                // Do something else...
            }
        }

        // Example of using the navigation stack
        void ShowStackedDialogs()
        {
            // Push first dialog
            var content1 = new SkiaLabel()
            {
                Text = "This is the first dialog",
                TextColor = Colors.White,
                FontSize = 16,
                HorizontalTextAlignment = DrawTextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
            };

            GameDialog.Push(this, content1, "NEXT", "CANCEL",
                onOk: () => {
                    // Push second dialog
                    var content2 = new SkiaLabel()
                    {
                        Text = "This is the second dialog",
                        TextColor = Colors.White,
                        FontSize = 16,
                        HorizontalTextAlignment = DrawTextAlignment.Center,
                        HorizontalOptions = LayoutOptions.Fill,
                    };

                    GameDialog.Push(this, content2, "FINISH", "BACK",
                        onOk: () => {
                            // Pop all dialogs
                            _ = GameDialog.PopAll(this);
                        },
                        onCancel: () => {
                            // Pop just this dialog (go back to first)
                            _ = GameDialog.Pop(this);
                        });
                },
                onCancel: () => {
                    // Cancel everything
                    _ = GameDialog.PopAll(this);
                });
        }

        /// <summary>
        /// Score can change several times per frame
        /// so we dont want bindings to update the score toooften.
        /// Instead we update the display manually once after the frame is finalized.
        /// </summary>
        void UpdateScore()
        {
            var collisionSystem = USE_RAYCAST_COLLISION ? "RAYCAST" : "AABB";
            LabelScore.Text = $"{ScoreLocalized} | {collisionSystem}";
            //LabelHiScore.Text = HiScoreLocalized;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddToPoolBrickSprite()
        {
            var brick = BrickSprite.Create();
            BricksPool.Add(brick.Uid, brick);
        }

        // Gameplay state
        private int _level = 1;

        public int Level
        {
            get => _level;
            set
            {
                if (_level != value)
                {
                    _level = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LevelDisplay));
                }
            }
        }

        public string LevelDisplay => $"LEVEL: {_level}";
        private int _lives = LIVES;

        public int Lives
        {
            get => _lives;
            set
            {
                if (_lives != value)
                {
                    _lives = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(LivesDisplay));
                }
            }
        }

        public string LivesDisplay => $"LIVES: {_lives}";
        private int _score;

        public int Score
        {
            get => _score;
            set
            {
                if (_score != value)
                {
                    _score = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ScoreLocalized));
                }
            }
        }

        public string ScoreLocalized => $"SCORE: {_score:0}";
        private bool _initialized;
        private bool _needPrerender;

        // Pools for bricks (reusable sprites)
        private Dictionary<Guid, BrickSprite> BricksPool = new(MAX_BRICKS);
        private Queue<SkiaControl> _spritesToBeRemovedLater = new();
        private object _lockSpritesToBeRemovedLater = new();
        private List<SkiaControl> _spritesToBeAdded = new(MAX_BRICKS);

        // For paddle movement via keys/gestures
        volatile bool _moveLeft, _moveRight;
        private bool _wasPanning;
        private bool _isPressed;
        private GameState _lastState;
        private GameState _state;

        public GameState State
        {
            get => _state;
            set
            {
                if (_state != value)
                {
                    PreviousState = _state;
                    _lastStateChecked = PreviousState;
                    _state = value;
                    OnPropertyChanged();
                    Debug.WriteLine($"Game state changed: {value}");
                }
            }
        }

        protected bool CheckStateChanged()
        {
            var ret = _lastStateChecked != State;
            _lastStateChecked = State;
            return ret;
        }

        protected GameState PreviousState;

        #region GAME LOOP

        public override void GameLoop(float deltaSeconds)
            //public void GameLoopBak(float deltaSeconds)
        {
            base.GameLoop(deltaSeconds);


            float cappedDelta = deltaSeconds; //Math.Min(deltaSeconds, 0.05f); 

            while (GameKeysQueue.Count > 0)
            {
                ApplyGameKey(GameKeysQueue.Dequeue());
            }


            if ((State == GameState.DemoPlay 
                 || State == GameState.Playing) && levelReady)
            {
                if (CheckStateChanged())
                {
                    PlaySound(Sound.Start);
                }

                // get the player hit box
                Ball.UpdateState(LastFrameTimeNanos);
                var ballRect = Ball.HitBox;

                bool ballCollided = false;
                int bricksChecked = 0;


                //----

                // collision detection
                foreach (var x in this.Views)
                {
                    //collide ball vs everything and update ball position
                    if (x is BallSprite ball && ball.IsActive)
                    {
                        //bricks
                        if (ball.IsMoving)
                        {
                            if (USE_RAYCAST_COLLISION)
                            {
                                // Use raycast collision detection
                                ballCollided = DetectCollisionsWithRaycast(ball, cappedDelta);

                                // Count bricks for level completion check
                                foreach (var view in Views)
                                {
                                    if (view is BrickSprite brick && brick.IsActive)
                                    {
                                        bricksChecked++;
                                    }
                                }
                            }
                            else
                            {
                                // Use traditional AABB collision detection
                                if (!ballCollided && ball.IsActive)
                                {
                                    foreach (var view in Views)
                                    {
                                        if (view is BrickSprite brick && brick.IsActive)
                                        {
                                            bricksChecked++;

                                            //calculate hitbox
                                            brick.UpdateState(LastFrameTimeNanos);

                                            if (ballRect.IntersectsWith(brick.HitBox, out var overlap))
                                            {
                                                CollideBallAndBrick(brick, ball, overlap);
                                                ballCollided = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            bricksChecked++;
                        }

                        //maybe GameState would change after all of that
                        if (BricksLeft == 0)
                        {
                            break;
                        }

                        if (State == GameState.Playing || State== GameState.DemoPlay)
                        {
                            //paddle(s) - only for traditional collision detection (raycast handles this in DetectCollisionsWithRaycast)
                            if (!USE_RAYCAST_COLLISION && !ballCollided && ball.IsActive)
                            {
                                if (MathF.Sin(ball.Angle) > 0) //only if ball moves downward
                                {
                                    foreach (var view in Views)
                                    {
                                        if (view is PaddleSprite paddle && paddle.IsActive)
                                        {
                                            //calculate hitbox
                                            paddle.UpdateState(LastFrameTimeNanos);

                                            if (ballRect.IntersectsWith(paddle.HitBox, out var overlap))
                                            {
                                                CollideBallAndPaddle(paddle, ball, overlap);
                                                ballCollided = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            //walls - only for traditional collision detection (raycast handles this in DetectCollisionsWithRaycast)
                            if (!USE_RAYCAST_COLLISION && !ballCollided && ball.IsActive)
                            {
                                if (ballRect.Left < 0)
                                {
                                    var penetration = -ballRect.Left;
                                    Ball.MoveOffset(penetration * 1.1f, 0);
                                    ball.Angle = MathF.PI - ball.Angle;
                                    PlaySound(Sound.Wall, new Vector3(-1.0f, 0f, -1f));
                                    ballCollided = true;
                                }
                                // Right wall
                                else
                                if (ballRect.Right > Width)
                                {
                                    var penetration = ballRect.Right - Width;
                                    Ball.MoveOffset(-penetration * 1.1f, 0);
                                    ball.Angle = MathF.PI - ball.Angle;
                                    PlaySound(Sound.Wall, new Vector3(2.0f, 0f, -1f));
                                    ballCollided = true;
                                }

                                // Top wall
                                if (ballRect.Top < 0)
                                {
                                    var penetration = -ballRect.Top;
                                    Ball.MoveOffset(0, penetration * 1.1f);
                                    ball.Angle = -ball.Angle;
                                    PlaySound(Sound.Wall);
                                    ballCollided = true;
                                }

                                if (ballRect.Bottom > Height)
                                {
                                    ballCollided = true;
                                    PlaySound(Sound.Oops);
                                    if (CHEAT_INVULNERABLE)
                                    {
                                        ResetBall();
                                    }
                                    else
                                    {
                                        //RemoveReusable(ball); //todo could have many balls lol

                                        //just 1 ball we have
                                        Lives--;
                                        if (Lives <= 0)
                                        {
                                            if (State == GameState.DemoPlay)
                                            {
                                                // In demo mode, restart from level 1 without showing dialog
                                                RestartDemoMode();
                                            }
                                            else
                                            {
                                                State = GameState.Ended;
                                                Task.Delay(1500).ContinueWith(_ =>
                                                {
                                                    MainThread.BeginInvokeOnMainThread(() => ShowGameOverDialog());
                                                });
                                            }
                                        }
                                        else
                                        {
                                            ResetBall();
                                            //State = GameState.Paused;
                                        }
                                    }
                                }
                            }

                            // Update ball position - raycast collision handles positioning during collision detection
                            if (ball.IsActive && (!ballCollided || !USE_RAYCAST_COLLISION))
                            {
                                ball.UpdatePosition(cappedDelta);
                            }
                        }
                    }
                }

                if (bricksChecked < 1 || BricksLeft == 0)
                {
                    _levelCompletionPending++;

                    //make sure we show few frames after the final collision was detected
                    if (_levelCompletionPending > 20) 
                    {
                        State = GameState.LevelComplete;
                        _levelCompletionPending = 0;
                    }
                }

                if (State == GameState.DemoPlay)
                {
                    AIController.UpdateAI(cappedDelta);
                }
                
                if (State == GameState.Playing || State == GameState.DemoPlay)
                {
                    // --

                    // player movement
                    if (_moveLeft)
                    {
                        UpdatePlayerPosition(Paddle.Left - PADDLE_SPEED * cappedDelta);
                    }

                    if (_moveRight)
                    {
                        UpdatePlayerPosition(Paddle.Left + PADDLE_SPEED * cappedDelta);
                    }

                    if (Lives < 1)
                    {
                        //EndGameLost();
                    }
                }
            }


            // removing sprites
            ProcessSpritesToBeRemoved();

            if (_spritesToBeAdded.Count > 0)
            {
                foreach (var add in _spritesToBeAdded)
                {
                    AddSubView(add);
                }

                _spritesToBeAdded.Clear();
            }

            UpdateScore();

            if (State == GameState.Playing || State == GameState.DemoPlay)
            {
                levelReady = true; //we have added all sprites, can play now
            }
            else if (State == GameState.LevelComplete)
            {
                LevelComplete();
            }
        }

        protected override void OnPaused()
        {
            StopLoop();
        }

        protected override void OnResumed()
        {
            if (State == GameState.Playing || State == GameState.DemoPlay)
            {
                StartLoop();
            }
        }

        private int _levelCompletionPending = 0;

        #endregion

        void CollideBallAndPaddle(PaddleSprite paddle, BallSprite ball)
        {
            PlaySound(Sound.Board);


            // Determine paddle's horizontal velocity based on current movement input
            float paddleVelocity = 0;
            if (_moveLeft)
                paddleVelocity = -PADDLE_SPEED;
            else if (_moveRight)
                paddleVelocity = PADDLE_SPEED;

            // Calculate where on the paddle the ball hit (normalized position from -1 to 1)
            float paddleWidth = paddle.HitBox.Width;
            float paddleCenterX = paddle.HitBox.MidX;
            float ballCenterX = ball.HitBox.MidX;

            // Calculate normalized hit position (-1 is far left, 0 is center, 1 is far right)
            float hitPosition = (ballCenterX - paddleCenterX) / (paddleWidth / 2);
            hitPosition = Math.Clamp(hitPosition, -1.0f, 1.0f);

            // Start with basic reflection
            float newAngle = -ball.Angle;

            // Track how horizontal the new angle will be (for speed adjustment)
            float horizontalFactor = 0f;
            float baseAngle = -MathF.PI / 3; // Straight up

            // Apply special angle changes if the paddle is moving
            if (Math.Abs(paddleVelocity) > 0.1f)
            {
                // Apply paddle velocity with appropriate direction
                float effectivePaddleVelocity = -paddleVelocity;

                // Influence factor 
                float velocityInfluence = 0.9f;

                // Calculate paddle effect
                float paddleEffect = (effectivePaddleVelocity / PADDLE_SPEED) * velocityInfluence;

                // Maximum angle adjustment (30 degrees)
                float maxAngleAdjust = MathF.PI / 8;

                // Calculate angle adjustment from paddle movement
                float velocityAngleAdjust = paddleEffect * maxAngleAdjust;

                // Apply hit position effect (larger effect toward the edges)
                float positionFactor = 0.66f; // !!! 
                float positionAngleEffect = hitPosition * MathF.PI / 9 * positionFactor;

                if (hitPosition < 0)
                {
                    // Combine for final angle
                    newAngle = (float)Math.PI - baseAngle + velocityAngleAdjust + positionAngleEffect;
                }
                else
                {
                    // Combine for final angle
                    newAngle = baseAngle + velocityAngleAdjust + positionAngleEffect;
                }
            }
            else
            {
                // For stationary paddle, use hit position-based reflection

                float angleRange = MathF.PI / 4;
                float adjustedHitPosition = MathF.Sign(hitPosition) * MathF.Pow(MathF.Abs(hitPosition), 1.9f);

                if (hitPosition < 0)
                {
                    // Combine for final angle
                    newAngle = (float)Math.PI - baseAngle + (adjustedHitPosition * angleRange);
                }
                else
                {
                    // Combine for final angle
                    newAngle = baseAngle + (adjustedHitPosition * angleRange);
                }
            }

            // Set the new angle
            ball.Angle = newAngle;

            // Adjust speed based on angle horizontality
            horizontalFactor = MathF.Abs(MathF.Cos(newAngle));
            float baseSpeedBoost = 1.03f;
            float angleSpeedBoost = 1.0f + (horizontalFactor * 0.33f);
            float paddleSpeedContribution = (Math.Abs(paddleVelocity) / PADDLE_SPEED) * 0.03f;
            float finalSpeedMultiplier = baseSpeedBoost * angleSpeedBoost + paddleSpeedContribution;

            // Apply speed change with cap
            float maxSpeedRatio = 1.1f + (horizontalFactor * 0.75f);
            ball.SpeedRatio = MathF.Min(ball.SpeedRatio * finalSpeedMultiplier, maxSpeedRatio);

            //adjust some for frame drops
            AlightBallWithPaddleSurface();

            if (Paddle.Powerup == PowerupType.StickyBall)
            {
                Ball.IsMoving = false;
            }
        }

        /// <summary>
        /// Handles collision between ball and paddle with contextual physics based on hit position
        /// Left half: Normal physics (paddle movement direction = ball direction)
        /// Right half: Inverse physics (paddle movement direction = opposite ball direction)
        /// </summary>
        void CollideBallAndPaddle(PaddleSprite paddle, BallSprite ball, SKRect overlap)
        {
            CollideBallAndPaddle(paddle, ball);
        }

        /// <summary>
        /// Handles collision between ball and brick
        /// </summary>
        void CollideBallAndBrick(BrickSprite brick, BallSprite ball, SKRect overlap)
        {
            // Get brick dimensions and position
            var brickRect = brick.HitBox;

            // Determine which face of the brick was hit
            var face = overlap.GetCollisionFace(brickRect);

            var penetration = overlap.Top;

            switch (face)
            {
                case CollisionFace.Top:
                case CollisionFace.Bottom:
                    penetration = overlap.Height;
                    break;

                case CollisionFace.Left:
                case CollisionFace.Right:
                    penetration = overlap.Width;
                    break;
            }

            CollideBallAndBrick(brick, ball, face, penetration);
        }

        void CollideBallAndBrick(BrickSprite brick, BallSprite ball, CollisionFace face, float overlap)
        {
            var offset = overlap * 1.1;

            switch (face)
            {
                case CollisionFace.Top:
                    ball.Angle = -ball.Angle;
                    ball.Top -= offset;
                    break;
                case CollisionFace.Bottom:
                    // Vertical collision - reflect vertically
                    ball.Angle = -ball.Angle;
                    ball.Top += offset;
                    break;

                case CollisionFace.Left:
                    ball.Left -= offset;
                    ball.Angle = MathF.PI - ball.Angle;
                    break;
                case CollisionFace.Right:
                    ball.Left += offset;
                    ball.Angle = MathF.PI - ball.Angle;
                    break;
            }

            // After calculating new angle in collision response
            // Move the ball slightly in the new direction to prevent sticking
            //float adjustDistance = overlap;
            //ball.Left += adjustDistance * MathF.Cos(ball.Angle);
            //ball.Top += adjustDistance * MathF.Sin(ball.Angle);

            // Handle brick hit logic based on properties
            if (brick.Undestructible)
            {
                PlaySound(Sound.Wall);
                // Don't remove undestructible bricks
                return;
            }

            PlaySound(Sound.Brick);

            if (brick.SupplementaryHitsToDestroy > 0)
            {
                // Brick needs multiple hits to destroy
                brick.SupplementaryHitsToDestroy--;
                // Add some visual feedback (could change color based on damage)
                // For now, just adjust opacity slightly
                brick.BackgroundColor = brick.BackgroundColor.WithLuminosity(0.5f);
                //brick.Opacity = 0.7f + (0.3f * brick.SupplementaryHitsToDestroy / 3.0f);

                // Add some points for hitting a reinforced brick
                Score += 5;
            }
            else
            {
                // Brick is destroyed
                // Increment score
                Score += 10;

                // Remove the brick
                RemoveBrick(brick);
            }
        }

        void RemoveBrick(BrickSprite brick)
        {
            BricksLeft -= 1;
            RemoveReusable(brick);
        }

        #region RAYCAST COLLISION DETECTION

        /// <summary>
        /// Alternative collision detection using raycasting for more accurate collision detection
        /// </summary>
        /// <param name="ball">The ball sprite</param>
        /// <param name="deltaSeconds">Time delta for this frame</param>
        /// <returns>True if a collision was detected and handled</returns>
        bool DetectCollisionsWithRaycast(BallSprite ball, float deltaSeconds)
        {
            if (!ball.IsActive || !ball.IsMoving)
                return false;

            // Calculate ball's movement for this frame
            // Use ball center position for raycast calculations
            Vector2 ballPosition = ball.Position; // This returns center position
            Vector2 ballDirection = ball.Direction;
            float ballRadius = (float)(ball.Width / 2);
            float ballSpeed = BALL_SPEED * ball.SpeedRatio;
            float maxDistance = ballSpeed * deltaSeconds;

            // System.Diagnostics.Debug.WriteLine($"Raycast: Ball center at ({ballPosition.X}, {ballPosition.Y}), direction ({ballDirection.X}, {ballDirection.Y}), distance {maxDistance}");

            // Collect all potential collision targets
            var collisionTargets = new List<IWithHitBox>();

            // Add bricks
            foreach (var view in Views)
            {
                if (view is BrickSprite brick && brick.IsActive)
                {
                    brick.UpdateState(LastFrameTimeNanos);
                    collisionTargets.Add(brick);
                }
            }

            // Add paddle (only if ball is moving downward)
            if (MathF.Sin(ball.Angle) > 0)
            {
                foreach (var view in Views)
                {
                    if (view is PaddleSprite paddle && paddle.IsActive)
                    {
                        paddle.UpdateState(LastFrameTimeNanos);
                        collisionTargets.Add(paddle);
                    }
                }
            }

            // Check for collisions with objects
            var objectHit = RaycastCollision.CastRay(ballPosition, ballDirection, maxDistance, ballRadius, collisionTargets);

            // Check for wall collisions
            var wallHit = RaycastCollision.CheckWallCollision(ballPosition, ballDirection, ballRadius, maxDistance, (float)Width, (float)Height);

            // Determine which collision happens first
            RaycastCollision.RaycastHit firstHit = RaycastCollision.RaycastHit.None;
            bool isWallCollision = false;

            if (objectHit.Collided && wallHit.Collided)
            {
                firstHit = objectHit.Distance <= wallHit.Distance ? objectHit : wallHit;
                isWallCollision = firstHit.Distance == wallHit.Distance;
            }
            else if (objectHit.Collided)
            {
                firstHit = objectHit;
                isWallCollision = false;
            }
            else if (wallHit.Collided)
            {
                firstHit = wallHit;
                isWallCollision = true;
            }

            if (firstHit.Collided)
            {
                // System.Diagnostics.Debug.WriteLine($"Raycast collision detected: {firstHit.Face} at distance {firstHit.Distance}");

                // Handle collision response - let the traditional collision methods handle positioning
                if (isWallCollision)
                {
                    HandleWallCollisionRaycast(ball, firstHit);
                }
                else
                {
                    HandleObjectCollisionRaycast(ball, firstHit);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles wall collision using raycast data - matches traditional collision behavior
        /// </summary>
        void HandleWallCollisionRaycast(BallSprite ball, RaycastCollision.RaycastHit hit)
        {
            // System.Diagnostics.Debug.WriteLine($"Wall collision: {hit.Face}");
            switch (hit.Face)
            {
            case CollisionFace.Left:
            // Check for shallow angle and use larger penetration if needed
            var leftPenetration = MathF.Abs(MathF.Cos(ball.Angle)) < 0.15f ? 6.0f : 2.0f;
            Ball.MoveOffset(leftPenetration * 1.1f, 0);
            ball.Angle = MathF.PI - ball.Angle;
            PlaySound(Sound.Wall, new Vector3(-1.0f, 0f, -1f));
            break;
            case CollisionFace.Right:
            // Check for shallow angle and use larger penetration if needed  
            var rightPenetration = MathF.Abs(MathF.Cos(ball.Angle)) < 0.15f ? 6.0f : 2.0f;
            Ball.MoveOffset(-rightPenetration * 1.1f, 0);
            ball.Angle = MathF.PI - ball.Angle;
            PlaySound(Sound.Wall, new Vector3(2.0f, 0f, -1f));
            break;
            case CollisionFace.Bottom:
            // Ball hit TOP wall (collision face is bottom of the wall)
            var topPenetration = 2.0f;
            Ball.MoveOffset(0, topPenetration * 1.1f);
            ball.Angle = -ball.Angle;
            PlaySound(Sound.Wall);
            break;
            case CollisionFace.Top:
            // Ball hit BOTTOM wall (collision face is top of the wall) - game over logic
            PlaySound(Sound.Oops);
            if (CHEAT_INVULNERABLE)
            {
                ResetBall();
            }
            else
            {
                Lives--;
                if (Lives <= 0)
                {
                    if (State == GameState.DemoPlay)
                    {
                        // In demo mode, restart from level 1 without showing dialog
                        RestartDemoMode();
                    }
                    else
                    {
                        State = GameState.Ended;
                        Task.Delay(1500).ContinueWith(_ =>
                        {
                            MainThread.BeginInvokeOnMainThread(() => ShowGameOverDialog());
                        });
                    }
                }
                else
                {
                    ResetBall();
                }
            }
            break;
            }
        }

        /// <summary>
        /// Handles object collision (brick or paddle) using raycast data
        /// </summary>
        void HandleObjectCollisionRaycast(BallSprite ball, RaycastCollision.RaycastHit hit)
        {
            if (hit.Target is BrickSprite brick)
            {
                // Use the SAME collision logic as traditional system
                // Calculate a small penetration value to match traditional behavior
                float fakePenetration = 2.0f; // Small value to simulate overlap
                CollideBallAndBrick(brick, ball, hit.Face, fakePenetration);
            }
            else if (hit.Target is PaddleSprite paddle)
            {
                // Use existing paddle collision logic
                CollideBallAndPaddle(paddle, ball);
            }
        }



        #endregion

        public void AlightBallWithPaddleSurface()
        {
            Ball.SetOffsetY(Paddle.Top - Ball.HeightRequest);
        }

        public void ResetBall()
        {
            //PlaySound(Sound.Start);

            Ball.SpeedRatio = 1;
            // Position the ball above the paddle's center
            Ball.SetOffsetX(Paddle.Left);

            AlightBallWithPaddleSurface();

            // Random angle between -60� and -120� (upward)
            float randomAngle = (float)(new Random().NextDouble() * (MathF.PI / 3) - MathF.PI / 6 - MathF.PI / 2);

            Ball.Angle = randomAngle;

            Ball.IsActive = true;
        }

        #region GESTURES AND KEYS

        public void ApplyGameKey(GameKey gameKey)
        {
            // For playing state, set movement flags
            if (State == GameState.Playing || State== GameState.DemoPlay)
            {
                if (gameKey == GameKey.Stop)
                {
                    _moveLeft = false;
                    _moveRight = false;
                }
                else
                if (gameKey == GameKey.Left)
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
                        // todo :)
                    }
                }

                //else if (mauiKey == MauiKey.Space)
                //{
                //    // If game is not started, start it; otherwise, you might serve the ball.
                //    if (State == GameState.Ready || State == GameState.Ended)
                //        ResetGame();
                //}
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

        bool TogglePause()
        {
            if (State == GameState.Playing)
            {
                State = GameState.Paused;
                _moveLeft = false;
                _moveRight = false;
                GameDialog.Show(this, null, "PAUSED", null, () =>
                {
                    TogglePause();
                });
                return true;
            }

            if (State == GameState.Paused)
            {
                State = GameState.Playing;
                _moveLeft = false;
                _moveRight = false;
                _ = GameDialog.PopAllAsync(this);
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
                System.Diagnostics.Debug.WriteLine($"Collision system switched to: {(USE_RAYCAST_COLLISION ? "RAYCAST" : "AABB")}");
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

        // Gesture handling following the SpaceShooter pattern.
        public override ISkiaGestureListener ProcessGestures(SkiaGesturesParameters args,
            GestureEventProcessingInfo apply)
        {
            if (GameDialog.IsAnyDialogOpen(this))
            {
                var consumed = base.ProcessGestures(args, apply);                
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
                    {
                        if (State == GameState.Playing)
                        {
                            GameKeysQueue.Enqueue(GameKey.Fire);
                        }
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

        #region HELPER METHODS

        void UpdatePlayerPosition(double x)
        {
            var leftLimit = -Width / 2f + Paddle.Width / 2f;
            var rightLimit = Width / 2f - Paddle.Width / 2f;
            var clampedX = Math.Clamp(x, leftLimit, rightLimit);
            var deltaX = clampedX - Paddle.Left;

            if (clampedX != Paddle.Left)
            {
                Paddle.Left = clampedX;

                if (!Ball.IsMoving)
                {
                    //move the bal too with us
                    Ball.MoveOffset(deltaX, 0);
                }

                //PlayerShield.Left = clampedX;
                //PlayerShieldExplosion.Left = clampedX;
                //HealthBar.Left = clampedX;
                Paddle.Repaint();
                Ball.Repaint();
            }
        }

        void RemoveReusable(IReusableSprite sprite)
        {
            sprite.IsActive = false;
            sprite.AnimateDisappearing()
                .ContinueWith((s) =>
                {
                    lock (_lockSpritesToBeRemovedLater)
                    {
                        if (!sprite.IsActive)
                            _spritesToBeRemovedLater.Enqueue(sprite as SkiaControl);
                    }
                }).ConfigureAwait(false);
        }

        void RemoveSprite(SkiaControl sprite)
        {
            if (sprite is BrickSprite enemy)
            {
                BricksPool.TryAdd(enemy.Uid, enemy);
            }

            RemoveSubView(sprite);
        }

        void ProcessSpritesToBeRemoved()
        {
            SkiaControl sprite;
            lock (_lockSpritesToBeRemovedLater)
            {
                while (_spritesToBeRemovedLater.Count > 0)
                {
                    if (_spritesToBeRemovedLater.TryDequeue(out sprite))
                    {
                        RemoveSprite(sprite);
                    }
                }
            }
        }

        #endregion
    }


 

}