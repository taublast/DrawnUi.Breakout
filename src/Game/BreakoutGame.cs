using AppoMobi.Maui.Gestures;
using AppoMobi.Specials;
using Breakout.Game.Ai;
using Breakout.Game.Dialogs;
using SkiaSharp;
using System.Diagnostics;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using static Breakout.Game.BreakoutGame;

namespace Breakout.Game
{
    public partial class BreakoutGame : MauiGame
    {
        #region CONSTANTS

        public const int WIDTH = 360;
        public const int HEIGHT = 760;

        public const float BALL_SPEED = 375f;
        public const float POWERUP_SPEED = 120;
        public const float PADDLE_SPEED = 475;
        public const float PADDLE_WIDTH = 80;
        public const int MAX_BRICKS_COLUMNS = 8;
        public const int MAX_BRICKS_ROWS = 15;
        public const int MAX_BRICKS = MAX_BRICKS_COLUMNS * MAX_BRICKS_ROWS + 1;
        public const int MIN_BRICKS_ROWS = 3;
        public const float SPACING_BRICKS = 3f;
        public const float BRICKS_SIDE_MARGIN = 16f;

        public const int LIVES = 3;
        private const double POWERUP_SPAWN_COOLDOWN_SECONDS = 0.2;

        public const int MAXLVL = 12;
        public const int DEMO_MAXLVL = 3;
        public const int POWERUP_MAX_BULLETS = 10;
        public const int POWERUP_DURATION = 10;

        public const int MAX_POWERUPS_IN_POOL = 12;
        public const int MAX_BULLETS_IN_POOL = 64;
        public const int MAX_BALLS_IN_POOL = 8;

        /// <summary>
        /// For long running profiling
        /// </summary>
        const bool CHEAT_INVULNERABLE = false;

        public static bool USE_SOUND = true;

        public static bool USE_SOUND_IN_DEMO = false;

        /// <summary>
        /// Compile-time flag to enable raycasting collision detection instead of AABB intersection
        /// AABB works ok on desktops, but on mobile with frame drops better to use raycasting.
        /// </summary>
        public static bool USE_RAYCAST_COLLISION = true;

        #endregion

        #region INITIALIZE

        private AIPaddleController _aiController;
        public AIPaddleController AIController => _aiController ??= new AIPaddleController(this, AIDifficulty.Medium);

        public IAudioService? _audioService;

        public BreakoutGame()
        {
            CreateUi();

#if ANDROID
            //prefer skipping frames than go smooth because this game is dynamic
            MauiGame.FrameInterpolatorDisabled = true;
#endif

            BindingContext = this;

            Instance = this;

            InitializeInput();

            InitDialogs();

            if (USE_SOUND)
            {
                _ = InitializeAudioAsync();
            }

            _aiController = new AIPaddleController(this, AIDifficulty.Hard);


            //pause/resume loop background music etc
            Super.OnNativeAppResumed += Super_OnNativeAppResumed;
            Super.OnNativeAppPaused += Super_OnNativeAppPaused;
        }

        #endregion

        public override void OnWillDisposeWithChildren()
        {
            Super.OnNativeAppResumed += Super_OnNativeAppResumed;

            Super.OnNativeAppPaused += Super_OnNativeAppPaused;

            _audioService?.Dispose();

            foreach (var inputController in InputControllers)
            {
                inputController.Dispose();
            }

            base.OnWillDisposeWithChildren();
        }

        private void Super_OnNativeAppPaused(object sender, EventArgs e)
        {
            Pause();
        }

        private void Super_OnNativeAppResumed(object sender, EventArgs e)
        {
            Resume();
        }

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
                Prepare(); //game loop will be started inside
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

        void Prepare()
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

            // Pool powerups for reuse
            for (int i = 0; i < MAX_POWERUPS_IN_POOL; i++)
            {
                AddToPoolPowerupSprite();
            }

            // Pool paddle bullets for reuse
            for (int i = 0; i < MAX_BULLETS_IN_POOL; i++)
            {
                AddToPoolPaddleBulletSprite();
            }

            // Pool balls for multiball powerup
            for (int i = 0; i < MAX_BALLS_IN_POOL; i++)
            {
                AddToPoolBallSprite();
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

        private BrickSprite AddBrickToContainer(string presetId, int col, int row,
            float brickWidth, float brickHeight, float margin)
        {
            var preset = BrickPresets.Presets[presetId];
            if (BricksPool.Count > 0)
            {
                var brick = BricksPool.Get();
                if (brick != null)
                {
                    brick.IsActive = true;
                    brick.WidthRequest = brickWidth;
                    brick.HeightRequest = brickHeight;

                    // Position relative to container (0,0)
                    float xPos = col * (brickWidth + margin);
                    float yPos = row * (brickHeight + margin);
                    brick.Left = xPos;
                    brick.Top = yPos;

                    brick.Preset = preset;
                    brick.BackgroundColor = preset.BackgroundColor;
                    brick.SupplementaryHitsToDestroy = preset.SupplementaryHitsToDestroy;
                    brick.Undestructible = preset.Undestructible;

                    // Add to container instead of main field
                    _spritesToBeAdded.Add(brick);
                    return brick;
                }
            }
            else
            {
                Super.Log("[FATAL] Out of bricks in the pool, check what you done wrong!!!");
            }

            return null;
        }

        private void AddBrick(string presetId, int col, int row,
            float brickWidth, float brickHeight, float margin, float offsetX, float offsetY)
        {
            var preset = BrickPresets.Presets[presetId];
            if (BricksPool.Count > 0)
            {
                var brick = BricksPool.Get();
                if (brick != null)
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

        public LevelManager LevelManager { get; set; }
        public int BricksLeftToBreak { get; set; }

        void LevelComplete()
        {
            if (_levelCompletionPrompt)
            {
                return;
            }

            _levelCompletionPrompt = true;

            // Store current game state before changing levels
            // Note: State is already GameState.LevelComplete at this point, so check PreviousState
            var wasInDemoMode = PreviousState == GameState.DemoPlay;

            if (wasInDemoMode)
            {
                // In demo mode, auto-continue to next level without showing dialog
                if (Level < DEMO_MAXLVL)
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
                if (Level < MAXLVL)
                {
                    Level++;
                    ShowLevelCompleteDialog();
                }
                else
                {
                    GameComplete();
                }
            }
        }

        public async void GameComplete()
        {
            PlaySound(Sound.Joy);

            var levelCompleteContent =
                UiElements.DialogPrompt(string.Format(ResStrings.MessageGameComplete, Score));

            if (await GameDialog.ShowAsync(this, levelCompleteContent, ResStrings.BtnOk.ToUpperInvariant()))
            {
                //todo can show credits or something

                StartDemoMode();
                ShowWelcomeDialog();
            }
        }

        /// <summary>
        /// Called when out of lives
        /// </summary>
        public void GameLost()
        {
            State = GameState.Ended;

            Tasks.StartDelayed(TimeSpan.FromMilliseconds(150), () =>
            {
                //PlaySound(Sound.SomethingTerrible);
                ShowGameOverDialog();
            });
        }

        private int CollectedPowerUps;
        private int CollectedPowerUpsSpeedy;
        private int BulletsAvailable;

        // Powerup spawn timing control
        private DateTime _lastPowerupSpawnTime = DateTime.MinValue;

        /// <summary>
        /// Start a precise level number in player mode
        /// </summary>
        /// <param name="level"></param>
        public void StartNewLevel(int level)
        {
            _ = GameDialog.PopAllAsync(this);
            Level = level;
            State = GameState.Unset;
            StartNewLevel();
        }

        void StartNewLevel()
        {
            if (LevelManager == null)
            {
                return;
            }

            _levelCompletionPrompt = false;
            _levelCompletionPending = 0;

            ClearSpritesOnBoard();
            ProcessSpritesToBeRemoved();

            ResetPaddle();

            // Then set ball properties for all active balls

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
                    formation = FormationType.Organic;
                    // null means use all available presets
                    break;

                case 4:
                    formation = FormationType.Diamond;
                    allowedPresets = new List<string>
                    {
                        "Standard_Red", "Standard_Blue", "Reinforced_Brown", "Hard_DarkGray"
                    };
                    break;

                case 5:
                    formation = FormationType.Maze;
                    // null means use all available presets
                    break;

                case 6:
                    formation = FormationType.Zigzag;
                    break;

                case 7:
                    formation = FormationType.Wave;
                    break;

                case 8:
                    formation = FormationType.Arch;
                    break;

                case 9:
                    formation = FormationType.Organic;
                    break;

                case 10:
                    formation = FormationType.Diamond;
                    break;

                case 11:
                    formation = FormationType.Zigzag;
                    break;

                case 12:
                    formation = FormationType.Grid;
                    break;

                default:
                    // Use modulo to cycle through all formations 
                    int formationIndex = Level % 8; // by total number of formations 
                    formation = (FormationType)formationIndex;
                    break;
            }

            // Generate the level
            var brickPositions = LevelManager.GenerateLevel(
                Level,
                (float)GameField.Width - BRICKS_SIDE_MARGIN * 2,
                (float)GameField.Height / 2,
                formation,
                allowedPresets
            );

            BricksLeftToBreak = LevelManager.CountBreakableBricks(brickPositions);

            // Calculate brick dimensions based on columns and rows
            int columns = brickPositions.Select(p => (int)p.Column).Distinct().Count();

            float margin = SPACING_BRICKS;
            float totalSpacing = margin * (columns + 1);
            float availableWidth = (float)Width - totalSpacing - BRICKS_SIDE_MARGIN * 2;
            float brickWidth = availableWidth / MAX_BRICKS_COLUMNS;
            float brickHeight = 20f; // Fixed brick height as in original code

            // Calculate container dimensions
            int maxRow = brickPositions.Max(p => (int)p.Row);

            float containerWidth = columns * brickWidth + (columns - 1) * margin;
            float containerHeight = (maxRow + 1) * brickHeight + maxRow * margin + 1;

            // Prepare BricksContainer
            SetupBricksContainer(containerWidth, containerHeight);

            // Create and setup BricksContainer
            // Add bricks to container
            foreach (var position in brickPositions)
            {
                int col = (int)position.Column;
                int row = (int)position.Row;
                string presetId = position.PresetId;

                if (string.IsNullOrEmpty(presetId))
                    continue;

                var brick = AddBrickToContainer(presetId, col, row, brickWidth, brickHeight, margin);

                if (brick != null)
                {
                    BrickPresets.ApplyPreset(brick, presetId);
                }
            }

            levelReady = false;

            // Preserve demo state if we're in demo mode, otherwise set to Playing
            if (PreviousState == GameState.Playing)
            {
                State = GameState.Playing;
            }

            if (State != GameState.Playing)
            {
                State = GameState.DemoPlay;
            }

            if (State == GameState.DemoPlay)
            {
                AIController.ResetTimers();
            }
            else
            {
                //PLAYER!
                State = GameState.Playing; //last one bulletrpoof
                PlaySound(Sound.Start);
            }

            SetupBackgroundMusic();
        }

        void SetupBricksContainer(float width, float height)
        {
            BricksContainer.ClearChildren(); //todo check they dont get disposed!

            BricksContainer.WidthRequest = width;
            BricksContainer.HeightRequest = height;
        }

        private bool levelReady;

        public void StartNewGameDemo()
        {
            PreviousState = GameState.DemoPlay;
            State = GameState.DemoPlay;
            StartNewGame();
        }

        public void StartNewGamePlayer()
        {
            PreviousState = GameState.Playing;
            State = GameState.Playing;
            StartNewGame();
        }


        void StartNewGame()
        {
            _ = GameDialog.PopAll(this);

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

            ShowWelcomeDialog();
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

        void ClearSpritesOnBoard()
        {
            lock (_lockSpritesToBeRemovedLater)
            {
                foreach (var control in GameField.Views)
                {
                    if (control == BricksContainer)
                    {
                        foreach (var brick in BricksContainer.Views)
                        {
                            _spritesToBeRemovedLater.Enqueue(brick);
                        }
                    }
                    else if (control is IReusableSprite)
                    {
                        _spritesToBeRemovedLater.Enqueue(control);
                    }
                }
            }
        }

        void RestartDemoMode()
        {
            // Restart demo mode from level 1 without showing any dialogs
            Score = 0;
            Lives = LIVES;
            Level = 1;

            ClearSpritesOnBoard();

            ProcessSpritesToBeRemoved();

            // Reset ball and continue demo
            ResetBall();
            foreach (var ball in ActiveBalls)
            {
                ball.IsMoving = false;
            }

            // Set demo state before starting new level
            State = GameState.DemoPlay;

            // Start new level in demo mode
            StartNewLevel();
            Update();
        }

        void ResetGame()
        {
            // Reset game state
            Lives = LIVES;
            Score = 0;
            Level = 1;
            State = GameState.Ready;

            ClearSpritesOnBoard();

            ProcessSpritesToBeRemoved();

            // Start new level
            StartNewLevel();
            Update();
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddToPoolBrickSprite()
        {
            var brick = BrickSprite.Create();
            BricksPool.Return(brick);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddToPoolPowerupSprite()
        {
            var powerup = PowerUpSprite.Create();
            PowerupsPool.Return(powerup);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddToPoolPaddleBulletSprite()
        {
            var bullet = BulletSprite.Create();
            PaddleBulletsPool.Return(bullet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddToPoolBallSprite()
        {
            var ball = BallSprite.Create();
            BallsPool.Return(ball);
        }

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
                }
            }
        }

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
                }
            }
        }

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
                }
            }
        }

        private bool _initialized;
        private bool _needPrerender;

        public class ReusableSpritePool<T> where T : IReusableSprite
        {
            protected Dictionary<Guid, T> Pool;

            public ReusableSpritePool(int size)
            {
                Pool = new(size);
            }

            public void Return(T item)
            {
                Pool.TryAdd(item.Uid, item);
            }

            public T Get()
            {
                var item = Pool.Values.FirstOrDefault();
                if (item != null && Pool.Remove(item.Uid))
                {
                    return item;
                }

                return default;
            }

            public int Count
            {
                get
                {
                    if (Pool == null)
                    {
                        return 0;
                    }

                    return Pool.Count;
                }
            }
        }

        // Pools for reusable sprites
        private ReusableSpritePool<BrickSprite> BricksPool = new(MAX_BRICKS);
        private ReusableSpritePool<PowerUpSprite> PowerupsPool = new(MAX_POWERUPS_IN_POOL);
        private ReusableSpritePool<BulletSprite> PaddleBulletsPool = new(MAX_BULLETS_IN_POOL);
        private ReusableSpritePool<BallSprite> BallsPool = new(MAX_BALLS_IN_POOL);
        public SKRect GameFieldArea = SKRect.Empty;
        public SKRect BricksArea = SKRect.Empty;
        private Queue<SkiaControl> _spritesToBeRemovedLater = new();
        private object _lockSpritesToBeRemovedLater = new();
        private List<SkiaControl> _spritesToBeAdded = new(MAX_BRICKS);

        // For paddle movement via keys/gestures
        volatile bool IsMovingLeft, IsMovingRight;
        private bool WasPanning;
        private bool IsPressed;

        // Ball stuck detection
        private Vector2 _lastBallPosition;
        private float _ballStuckTimer;
        private const float BALL_STUCK_THRESHOLD = 2.0f; // seconds
        private const float BALL_STUCK_DISTANCE = 5.0f; // pixels
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


        void CollideBallAndPaddle(PaddleSprite paddle, BallSprite ball)
        {
            PlaySound(Sound.Board);

            // Determine paddle's horizontal velocity based on current movement input
            float paddleVelocity = 0;
            if (IsMovingLeft)
                paddleVelocity = -PADDLE_SPEED;
            else if (IsMovingRight)
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
            AlightBallWithPaddleSurface(ball);

            if (Paddle.Powerup == PowerupType.StickyBall)
            {
                // Make THIS specific ball sticky (the one that just collided)
                ball.IsMoving = false;
                // Position sticky ball above paddle
                ball.SetOffsetY(Paddle.Top - ball.HeightRequest);
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
        void CollideBallAndBrick(BrickSprite brick, BallSprite ball, SKRect overlap, bool isFireball = false)
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

            CollideBallAndBrick(brick, ball, face, penetration, isFireball);
        }

        void CollideBallAndBrick(BrickSprite brick, BallSprite ball, CollisionFace face, float overlap,
            bool isFireball = false)
        {
            var offset = overlap * 1.1;

            // Only bounce if not in fireball mode
            if (!isFireball)
            {
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
            }

            // After calculating new angle in collision response
            // Move the ball slightly in the new direction to prevent sticking
            //float adjustDistance = overlap;
            //ball.Left += adjustDistance * MathF.Cos(ball.Angle);
            //ball.Top += adjustDistance * MathF.Sin(ball.Angle);

            // Handle brick hit logic based on properties
            if (brick.Undestructible)
            {
                if (isFireball)
                {
                    // Fireball can pass through undestructible bricks too!
                    PlaySound(Sound.Brick); // Different sound for fireball
                    // Don't remove undestructible bricks, but don't return either
                    // Let fireball continue through
                }
                else
                {
                    PlaySound(Sound.Wall);
                    // Don't remove undestructible bricks
                    return;
                }
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

                SpawnPowerUp(brick);

                // Remove the brick
                RemoveBrick(brick);
            }
        }

        void RemoveBrick(BrickSprite brick)
        {
            // Only decrement counter for destructible bricks
            if (!brick.Undestructible)
            {
                BricksLeftToBreak -= 1;
            }

            RemoveReusable(brick);
        }

        void CollideBulletAndBrick(BulletSprite bullet, BrickSprite brick)
        {
            // Remove the bullet
            RemoveReusable(bullet);

            // Handle brick hit logic - destroyer bullets can destroy undestructible bricks!
            if (brick.Undestructible)
            {
                // Destroyer bullets can destroy even undestructible bricks!
                PlaySound(Sound.Brick); // Different sound for destroyer
                Score += 10; // Give points for destroying undestructible brick
                SpawnPowerUp(brick);
                RemoveBrick(brick); // This will NOT decrement BricksLeftToBreak due to our fix
                return; // Exit early, brick is destroyed
            }

            PlaySound(Sound.Brick);

            if (brick.SupplementaryHitsToDestroy > 0)
            {
                brick.SupplementaryHitsToDestroy--;
                brick.BackgroundColor = brick.BackgroundColor.WithLuminosity(0.5f);
                Score += 5;
            }
            else
            {
                Score += 10;

                SpawnPowerUp(brick);

                RemoveBrick(brick);
            }
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
            foreach (var view in GameField.Views)
            {
                if (view == BricksContainer)
                {
                    foreach (var child in BricksContainer.Views)
                    {
                        if (child is BrickSprite brick && brick.IsActive)
                        {
                            collisionTargets.Add(brick);
                        }
                    }
                }
            }

            // Add paddle (only if ball is moving downward)
            if (MathF.Sin(ball.Angle) > 0)
            {
                foreach (var view in GameField.Views)
                {
                    if (view is PaddleSprite paddle && paddle.IsActive)
                    {
                        paddle.UpdateState(LastFrameTimeNanos);
                        collisionTargets.Add(paddle);
                    }
                }
            }

            // Check for collisions with objects
            var objectHit =
                RaycastCollision.CastRay(ballPosition, ballDirection, maxDistance, ballRadius, collisionTargets);

            // Check for wall collisions
            var wallHit = RaycastCollision.CheckWallCollision(ballPosition, ballDirection, ballRadius, maxDistance,
                GameField.VisualLayer.HitBoxWithTransforms.Units);

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
                    ball.MoveOffset(leftPenetration * 1.1f, 0);
                    ball.Angle = MathF.PI - ball.Angle;
                    PlaySound(Sound.Wall, new Vector3(-1.0f, 0f, -1f));
                    break;
                case CollisionFace.Right:
                    // Check for shallow angle and use larger penetration if needed
                    var rightPenetration = MathF.Abs(MathF.Cos(ball.Angle)) < 0.15f ? 6.0f : 2.0f;
                    ball.MoveOffset(-rightPenetration * 1.1f, 0);
                    ball.Angle = MathF.PI - ball.Angle;
                    PlaySound(Sound.Wall, new Vector3(2.0f, 0f, -1f));
                    break;
                case CollisionFace.Bottom:
                    // Ball hit TOP wall (collision face is bottom of the wall)
                    var topPenetration = 2.0f;
                    ball.MoveOffset(0, topPenetration * 1.1f);
                    ball.Angle = -ball.Angle;
                    PlaySound(Sound.Wall);
                    break;
                case CollisionFace.Top:
                    // Ball hit BOTTOM wall (collision face is top of the wall) - game over logic
                    PlaySound(Sound.Oops);
                    if (CHEAT_INVULNERABLE)
                    {
                        // In cheat mode, just reset the ball position
                        ball.Top = GameField.Height - ball.Height - 10;
                        ball.Angle = -ball.Angle;
                    }
                    else
                    {
                        // Remove this ball from play (multiball-aware)
                        RemoveBall(ball);

                        // Check if all balls are gone
                        if (ActiveBalls.Count == 0)
                        {
                            LooseLife();
                        }
                    }

                    break;
            }
        }

        void LooseLife()
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
                    this.GameLost();
                }
            }
            else
            {
                ResetPaddle(false);
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
                CollideBallAndBrick(brick, ball, hit.Face, fakePenetration, ball.IsFireball);
            }
            else if (hit.Target is PaddleSprite paddle)
            {
                // Use existing paddle collision logic
                CollideBallAndPaddle(paddle, ball);
            }
        }

        #endregion

        public void AlightBallWithPaddleSurface(BallSprite ball = null)
        {
            if (ball != null)
            {
                ball.SetOffsetY(Paddle.Top - ball.HeightRequest);
            }
            else
            {
                // If no specific ball provided, align all active balls
                foreach (var activeBall in ActiveBalls)
                {
                    activeBall.SetOffsetY(Paddle.Top - activeBall.HeightRequest);
                }
            }
        }

        public void ResetPaddle(bool center = true)
        {
            CollectedPowerUps = 0;
            CollectedPowerUpsSpeedy = 0;

            IsMovingLeft = false;
            IsMovingRight = false;

            if (center)
                Paddle.Left = 0;

            ResetBall();

            ResetPowerUp();
        }

        void ResetPowerUp()
        {
            foreach (var ball in ActiveBalls)
            {
                ball.IsFireball = false;
            }
            ResetBallsSpeed();

            Paddle.Powerup = PowerupType.None;
            Paddle.PowerupDuration = 0;
        }

        public void ResetBall()
        {
            //PlaySound(Sound.Start);

            // Clear all existing balls
            ClearAllBalls();

            // Add a single ball
            var newBall = AddBall();
            if (newBall != null)
            {
                newBall.SpeedRatio = 1;
                // Position the ball above the paddle's center
                newBall.SetOffsetX(Paddle.Left);

                AlightBallWithPaddleSurface(newBall);

                // Random angle between -60� and -120� (upward)
                float randomAngle = (float)(new Random().NextDouble() * (MathF.PI / 3) - MathF.PI / 6 - MathF.PI / 2);

                newBall.Angle = randomAngle;

                newBall.IsMoving = false;
                newBall.IsActive = true;
            }

            ResetBallsSpeed();
        }

        void ResetBallsSpeed()
        {
            foreach (var ball in ActiveBalls)
            {
                ball.SpeedRatio = 1 + 0.05f * (Level - 1);
            }
        }

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

                // Move ALL sticky balls with the paddle
                foreach (var ball in ActiveBalls)
                {
                    if (!ball.IsMoving)
                    {
                        ball.MoveOffset(deltaX, 0);
                    }
                }
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
                BricksPool.Return(enemy);
            }

            if (sprite is PowerUpSprite powerup)
            {
                PowerupsPool.Return(powerup);
            }

            if (sprite is BulletSprite paddleBullet)
            {
                PaddleBulletsPool.Return(paddleBullet);
            }

            if (sprite is BrickSprite brick)
            {
                BricksContainer.RemoveSubView(brick);
            }
            else
            {
                GameField.RemoveSubView(sprite);
            }
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

        public void SpawnPowerUp(BrickSprite brick)
        {
            // Check if enough time has passed since last powerup spawn
            var currentTime = DateTime.Now;
            var timeSinceLastSpawn = (currentTime - _lastPowerupSpawnTime).TotalSeconds;

            if (timeSinceLastSpawn < POWERUP_SPAWN_COOLDOWN_SECONDS)
            {
                // Too soon, skip spawning this powerup
                return;
            }

            // Update timestamp immediately to prevent multiple spawns in same frame
            _lastPowerupSpawnTime = currentTime;

            if (PowerupsPool.Count > 0)
            {
                var powerup = PowerupsPool.Get();
                if (powerup != null)
                {
                    powerup.IsActive = true;

                    // Determine powerup type
                    PowerupType powerupType = PowerupType.None;
                    if (brick.Preset != null && brick.Preset.PowerUpType != PowerupType.None)
                    {
                        var chance = RndExtensions.Rnd.NextDouble(0, 1, 0.01);
                        if (chance < 0.5)
                        {
                            powerupType = brick.Preset.PowerUpType;
                        }
                    }
                    else
                    {
                        powerupType = GetRandomPowerupType();
                    }

                    if (powerupType != PowerupType.None)
                    {
                        // Position at center of destroyed brick
                        powerup.Left = brick.HitBox.Left + (brick.Width - powerup.WidthRequest) / 2 -
                                       GameFieldArea.Left;
                        powerup.Top = brick.HitBox.Top - GameFieldArea.Top;

                        powerup.SetPowerupType(powerupType);
                        powerup.ResetAnimationState();

                        _spritesToBeAdded.Add(powerup);
                    }
                    else
                    {
                        PowerupsPool.Return(powerup);
                    }
                }
            }
        }

        private PowerupType GetRandomPowerupType()
        {
            var chance = RndExtensions.Rnd.NextDouble(0, 1, 0.01);

            if (chance < 0.05) return PowerupType.ExtraLife;
            if (chance < 0.10) return PowerupType.Destroyer;
            if (chance < 0.15) return PowerupType.MultiBall;
            if (chance < 0.17) return PowerupType.Fireball;
            if (chance < 0.27) return PowerupType.SlowBall;
            if (chance < 0.37) return PowerupType.ShrinkPaddle;
            if (chance < 0.47) return PowerupType.FastBall;
            if (chance < 0.57) return PowerupType.ExpandPaddle;
            if (chance < 0.67) return PowerupType.StickyBall;

            return PowerupType.None;
        }

        private void ApplyPowerUp(PowerupType powerUpType)
        {
            if (powerUpType != PowerupType.None)
            {
                CollectedPowerUps++;

                // Add score for collecting powerups
                switch (powerUpType)
                {
                    case PowerupType.MultiBall:
                    case PowerupType.Fireball:
                        Score += 200;
                        break;
                    case PowerupType.Destroyer:
                        Score += 150;
                        break;
                    case PowerupType.ExpandPaddle:
                    case PowerupType.StickyBall:
                    case PowerupType.ExtraLife:
                    case PowerupType.SlowBall:
                        Score += 30;
                        break;
                    case PowerupType.ShrinkPaddle:
                    case PowerupType.FastBall:
                        Score += 100;
                        break;
                }

                if (powerUpType == PowerupType.Destroyer || powerUpType == PowerupType.FastBall)
                {
                    CollectedPowerUpsSpeedy++;
                    if (CollectedPowerUpsSpeedy == 1)
                    {
                        PlaySpeedyMusic();
                    }
                }
                else //rare
                if (powerUpType == PowerupType.MultiBall || powerUpType == PowerupType.Fireball)
                {
                    CollectedPowerUpsSpeedy++;
                    if (CollectedPowerUpsSpeedy == 1)
                    {
                        PlaySpecialMusic();
                    }
                }
            }

            if (powerUpType == PowerupType.Destroyer)
            {
                BulletsAvailable = POWERUP_MAX_BULLETS;
                PlaySound(Sound.Attack);
            }
            else if (powerUpType == PowerupType.None)
            {
                PlaySound(Sound.PowerDown);
            }
            else
            {
                PlaySound(Sound.PowerUp);
            }

            // Reset previous powerup effects before applying new one
            if (Paddle.Powerup != PowerupType.None && Paddle.Powerup != powerUpType)
            {
                ResetPowerUp();
            }


            if (powerUpType == PowerupType.ExtraLife)
            {
                if (Lives < 8)
                {
                    Lives++;
                }
            }

            // Apply ball speed effects to all active balls
            if (powerUpType == PowerupType.SlowBall)
            {
                foreach (var ball in ActiveBalls)
                {
                    ball.SpeedRatio *= 0.5f;
                }
            }
            else if (powerUpType == PowerupType.FastBall)
            {
                foreach (var ball in ActiveBalls)
                {
                    ball.SpeedRatio *= 2.0f;
                }
            }
            else if (powerUpType == PowerupType.MultiBall)
            {
                ActivateMultiball();
            }
            else if (powerUpType == PowerupType.Fireball)
            {
                ActivateFireball();
            }

            // Handle sticky ball logic - release all balls when switching away from sticky
            if (Paddle.Powerup == PowerupType.StickyBall && powerUpType != PowerupType.StickyBall)
            {
                foreach (var ball in ActiveBalls)
                {
                    ball.IsMoving = true;
                }
            }
            else if (powerUpType != PowerupType.StickyBall)
            {
                foreach (var ball in ActiveBalls)
                {
                    ball.IsMoving = true;
                }
            }

            Debug.WriteLine($"POWERUP! {powerUpType}");

            Paddle.Powerup = powerUpType;
        }

        /// <summary>
        /// Activates multiball powerup by spawning additional balls
        /// </summary>
        private void ActivateMultiball()
        {
            if (ActiveBalls.Count == 0) return;

            // Get the first active ball as reference
            var referenceBall = ActiveBalls.First();

            // Make sure all balls start moving (override sticky ball)
            foreach (var ball in ActiveBalls)
            {
                ball.IsMoving = true;
            }

            // Spawn 2 additional balls (total of 3 balls)
            const int additionalBalls = 2;
            const float angleSpread = MathF.PI / 4; // 45 degrees spread

            for (int i = 0; i < additionalBalls; i++)
            {
                var newBall = AddBall();
                if (newBall != null)
                {
                    // Position new ball at reference ball location
                    newBall.Left = referenceBall.Left;
                    newBall.Top = referenceBall.Top;

                    // Copy reference ball properties
                    newBall.SpeedRatio = referenceBall.SpeedRatio;
                    newBall.IsMoving = true; // Always start moving, ignore sticky ball
                    newBall.IsActive = true;
                    newBall.IsFireball = referenceBall.IsFireball; // Inherit fireball state

                    // Calculate spread angle
                    float baseAngle = referenceBall.Angle;
                    float spreadOffset = (i + 1) * (angleSpread / (additionalBalls + 1)) - angleSpread / 2;
                    newBall.Angle = baseAngle + spreadOffset;

                    // Ensure angle is valid
                    newBall.Angle = BallSprite.ClampAngleFromHorizontal(newBall.Angle);

                    Debug.WriteLine($"Multiball: Spawned ball {i + 1} at angle {newBall.Angle * 180 / MathF.PI:F1}°");
                }
            }

            Debug.WriteLine($"Multiball activated! Total balls: {ActiveBalls.Count}");
        }

        /// <summary>
        /// Activates fireball powerup making all balls destructive and able to pass through bricks
        /// </summary>
        private void ActivateFireball()
        {
            // Make all active balls fireballs
            foreach (var ball in ActiveBalls)
            {
                ball.IsFireball = true;
            }

            Debug.WriteLine($"Fireball activated! All {ActiveBalls.Count} balls are now fireballs");
        }

        private bool DetectBulletCollisionsWithRaycast(BulletSprite bullet, float deltaSeconds)
        {
            // Calculate bullet movement
            Vector2 bulletPosition = new Vector2((float)bullet.HitBox.Left + (float)bullet.HitBox.Width / 2,
                (float)bullet.HitBox.Top + (float)bullet.HitBox.Height / 2);

            Vector2 bulletDirection = new Vector2(0, -1); // Moving up
            float bulletRadius = (float)bullet.HitBox.Width / 2;
            float bulletSpeed = BulletSprite.Speed * bullet.SpeedRatio;
            float maxDistance = bulletSpeed * deltaSeconds;

            // Collect brick targets
            var collisionTargets = new List<IWithHitBox>();

            foreach (var view in GameField.Views)
            {
                if (view == BricksContainer)
                {
                    foreach (var child in BricksContainer.Views)
                    {
                        if (child is BrickSprite brick && brick.IsActive)
                        {
                            collisionTargets.Add(brick);
                        }
                    }
                }
                // Add powerups as targets too
                else if (view is PowerUpSprite powerup && powerup.IsActive)
                {
                    collisionTargets.Add(powerup);
                }
            }

            // Check for collisions
            var hit = RaycastCollision.CastRay(bulletPosition, bulletDirection, maxDistance, bulletRadius,
                collisionTargets);

            if (hit.Collided)
            {
                if (hit.Target is BrickSprite brickHit)
                {
                    CollideBulletAndBrick(bullet, brickHit);
                    return true;
                }
                else if (hit.Target is PowerUpSprite powerupHit)
                {
                    // Remove both bullet and powerup
                    RemoveReusable(bullet);
                    RemoveReusable(powerupHit);
                    return true;
                }
            }

            return false;
        }

        void FirePaddleBullet()
        {
            if (PaddleBulletsPool.Count > 0)
            {
                var bullet = PaddleBulletsPool.Get();
                if (bullet != null)
                {
                    bullet.IsActive = true;

                    // Position bullet at center of paddle, above it
                    bullet.Left = Paddle.Left + (Paddle.Width - bullet.Width) / 2;
                    bullet.Top = Paddle.Top - bullet.Height;

                    bullet.ResetAnimationState();
                    _spritesToBeAdded.Add(bullet);
                }
            }

            BulletsAvailable--;
            if (BulletsAvailable <= 0)
            {
                ApplyPowerUp(PowerupType.None);
            }
        }

        #endregion
    }
}