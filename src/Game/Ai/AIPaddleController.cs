using Breakout.Game.Input;

namespace Breakout.Game.Ai
{
    /// <summary>
    /// Controls the paddle in demo mode, simulating a human player with variable skill
    /// </summary>
    public class AIPaddleController
    {
        private readonly BreakoutGame _game;
        private readonly Random _random = new Random();
        private double _targetX;
        private float _reactionTimer;
        private float _mistakeTimer;
        private float _decisionChangeTimer;
        private float _movementSmoothingTimer;
        private bool _makingMistake;
        private float _mistakeDirection;
        private bool _canFire = true;
        private bool _isMoving = false;
        private GameKey _lastMovementKey = GameKey.Stop;
        private float _idleWanderTimer = 0;
        private float _idleWanderInterval = 0.8f;
        private float _shootingTimer = 0;
        private float _shootingInterval = 0.3f; // Shoot every 300ms when in destroyer mode

        // AI characteristics
        private readonly float _reactionTimeMin;
        private readonly float _reactionTimeMax;
        private readonly float _accuracy;
        private readonly float _mistakeProbability;
        private readonly float _mistakeDurationMin;
        private readonly float _mistakeDurationMax;
        private float _decisionChangeInterval;
        private readonly float _serveDelay;
        private readonly float _movementSmoothingTime;

        /// <summary>
        /// Creates a new AI paddle controller with specified difficulty
        /// </summary>
        /// <param name="game">Reference to the main game</param>
        /// <param name="difficulty">AI difficulty level</param>
        public AIPaddleController(BreakoutGame game, AIDifficulty difficulty = AIDifficulty.Medium)
        {
            _game = game;

            // Set AI characteristics based on difficulty
            switch (difficulty)
            {
            case AIDifficulty.Easy:
            _reactionTimeMin = 0.5f;
            _reactionTimeMax = 1.2f;
            _accuracy = 0.6f;
            _mistakeProbability = 0.4f;
            _mistakeDurationMin = 0.8f;
            _mistakeDurationMax = 1.5f;
            _decisionChangeInterval = 1.0f;
            _serveDelay = 2.0f;
            _movementSmoothingTime = 0.3f;
            break;

            case AIDifficulty.Hard:
            _reactionTimeMin = 0.05f;  // Much faster reaction
            _reactionTimeMax = 0.15f;  // Reduced from 0.4f
            _accuracy = 0.98f;         // Increased from 0.9f
            _mistakeProbability = 0.02f; // Reduced from 0.1f
            _mistakeDurationMin = 0.1f;  // Reduced from 0.2f
            _mistakeDurationMax = 0.2f;  // Reduced from 0.5f
            _decisionChangeInterval = 3.0f; // Increased from 2.0f
            _serveDelay = 0.5f;        // Reduced from 0.8f
            _movementSmoothingTime = 0.05f; // Reduced from 0.1f
            break;

            case AIDifficulty.Medium:
            default:
            _reactionTimeMin = 0.25f;
            _reactionTimeMax = 0.7f;
            _accuracy = 0.75f;
            _mistakeProbability = 0.2f;
            _mistakeDurationMin = 0.4f;
            _mistakeDurationMax = 0.9f;
            _decisionChangeInterval = 1.5f;
            _serveDelay = 1.5f;
            _movementSmoothingTime = 0.2f;
            break;

            case AIDifficulty.Perfect:
            _reactionTimeMin = 0.01f;
            _reactionTimeMax = 0.02f;
            _accuracy = 1.0f;          // Perfect accuracy
            _mistakeProbability = 0.0f; // No mistakes
            _mistakeDurationMin = 0.0f;
            _mistakeDurationMax = 0.0f;
            _decisionChangeInterval = 5.0f;
            _serveDelay = 0.2f;
            _movementSmoothingTime = 0.01f;
            break;
            }

            ResetTimers();
        }

        /// <summary>
        /// Resets all AI decision timers
        /// </summary>
        public void ResetTimers()
        {
            _reactionTimer = GetRandomReactionTime();
            _mistakeTimer = 0;
            _decisionChangeTimer = _serveDelay; // Initial delay before serving
            _movementSmoothingTimer = 0;
            _makingMistake = false;
            _mistakeDirection = 0;
            _canFire = true;
            _isMoving = false;
            _lastMovementKey = GameKey.Stop;
            _idleWanderTimer = 0;
            _shootingTimer = 0;
        }

        /// <summary>
        /// Updates the AI paddle position based on ball trajectory and AI characteristics
        /// </summary>
        /// <param name="deltaTime">Time since last frame</param>
        public void UpdateAI(float deltaTime)
        {
            // Update shooting timer for destroyer mode
            _shootingTimer -= deltaTime;

            // If paddle has destroyer powerup, shoot at bricks
            if (_game.Paddle.Powerup == PowerupType.Destroyer && _shootingTimer <= 0)
            {
                _game.ApplyGameKey(GameKey.Fire);
                _shootingTimer = _shootingInterval + _random.NextSingle() * 0.2f;
            }

            // If no balls are moving, handle serving
            if (_game.ActiveBalls.Count == 0 || !_game.ActiveBalls.Any(b => b.IsMoving))
            {
                IdleWandering(deltaTime);
                _decisionChangeTimer -= deltaTime;
                if (_decisionChangeTimer <= 0 && _canFire)
                {
                    _game.ApplyGameKey(GameKey.Fire);
                    _canFire = false;
                    _decisionChangeTimer = _decisionChangeInterval;
                }
                return;
            }

            _canFire = true;
            _reactionTimer -= deltaTime;
            _movementSmoothingTimer -= deltaTime;

            // Handle mistakes
            if (_makingMistake)
            {
                _mistakeTimer -= deltaTime;
                if (_mistakeTimer <= 0)
                {
                    _makingMistake = false;
                    _reactionTimer = GetRandomReactionTime();

                    // Stop movement after mistake
                    StopMovement();
                }
                else
                {
                    // Apply mistake movement
                    if (_mistakeDirection < 0 && _lastMovementKey != GameKey.Left)
                    {
                        _game.ApplyGameKey(GameKey.Left);
                        _lastMovementKey = GameKey.Left;
                        _isMoving = true;
                    }
                    else if (_mistakeDirection > 0 && _lastMovementKey != GameKey.Right)
                    {
                        _game.ApplyGameKey(GameKey.Right);
                        _lastMovementKey = GameKey.Right;
                        _isMoving = true;
                    }
                    else if (_mistakeDirection == 0 && _isMoving)
                    {
                        StopMovement();
                    }
                    return;
                }
            }

            _decisionChangeTimer -= deltaTime;
            if (_decisionChangeTimer <= 0)
            {
                // Decide whether to make a mistake
                if (!_makingMistake && _random.NextDouble() < _mistakeProbability)
                {
                    _makingMistake = true;
                    _mistakeTimer = _random.NextSingle() *
                        (_mistakeDurationMax - _mistakeDurationMin) + _mistakeDurationMin;

                    // Either move in a random direction or stay still
                    if (_random.NextDouble() < 0.7)
                    {
                        _mistakeDirection = _random.NextDouble() < 0.5 ? -1 : 1;
                    }
                    else
                    {
                        _mistakeDirection = 0; // Stay still as a "mistake"
                    }
                }

                _decisionChangeInterval = 0.5f + _random.NextSingle();
                _decisionChangeTimer = _decisionChangeInterval;
            }

            // If making a mistake, continue with that
            if (_makingMistake)
            {
                if (_mistakeDirection < 0 && _lastMovementKey != GameKey.Left)
                {
                    _game.ApplyGameKey(GameKey.Left);
                    _lastMovementKey = GameKey.Left;
                    _isMoving = true;
                }
                else if (_mistakeDirection > 0 && _lastMovementKey != GameKey.Right)
                {
                    _game.ApplyGameKey(GameKey.Right);
                    _lastMovementKey = GameKey.Right;
                    _isMoving = true;
                }
                else if (_mistakeDirection == 0 && _isMoving)
                {
                    StopMovement();
                }
                return;
            }

            // Find the closest ball that's coming down
            var closestBall = _game.ActiveBalls
                .Where(ball => ball.IsMoving && MathF.Sin(ball.Angle) > 0) // Coming down
                .OrderBy(ball => ball.Top) // Closest to paddle
                .FirstOrDefault();

            if (closestBall != null)
            {
                // BALL IS COMING DOWN - PRIORITIZE BALL, but try powerups if safe
                if (_reactionTimer <= 0)
                {
                    // Calculate ball trajectory first
                    var ballX = closestBall.Left;
                    var ballY = closestBall.Top;
                    var paddleY = _game.Paddle.Top;

                    var ballVelX = MathF.Cos(closestBall.Angle) * BreakoutGame.BALL_SPEED * closestBall.SpeedRatio;
                    var ballVelY = MathF.Sin(closestBall.Angle) * BreakoutGame.BALL_SPEED * closestBall.SpeedRatio;

                    var timeToIntersect = (paddleY - ballY) / ballVelY;

                    if (timeToIntersect > 0)
                    {
                        var predictedX = ballX + ballVelX * timeToIntersect;
                        var accuracyBonus = _game.Paddle.Powerup == PowerupType.Destroyer ? 0.1f : 0f;
                        var effectiveAccuracy = Math.Min(1.0f, _accuracy + accuracyBonus);
                        var maxError = (1 - effectiveAccuracy) * _game.Paddle.Width;
                        var error = (_random.NextSingle() * 2 - 1) * maxError;

                        // Handle wall bounces
                        var halfGameWidth = _game.Width / 2;
                        while (predictedX < -halfGameWidth || predictedX > halfGameWidth)
                        {
                            if (predictedX < -halfGameWidth)
                                predictedX = -halfGameWidth - (predictedX + halfGameWidth);
                            else
                                predictedX = halfGameWidth - (predictedX - halfGameWidth);
                        }

                        _targetX = predictedX + error;
                        _reactionTimer = GetRandomReactionTime();

                        // Check if we have time to grab a powerup on the way
                        if (timeToIntersect > 2.0f && CheckForPowerupsToCollect())
                        {
                            // Go for powerup if we have plenty of time
                            return;
                        }
                        
                        // Otherwise focus on ball
                        MovePaddleTowardTarget();
                        return;
                    }
                }
            }
            else
            {
                // BALL IS FLYING UP - SAFE! PRIORITIZE POWERUPS!
                if (CheckForPowerupsToCollect())
                {
                    return; // Going for powerup
                }
                
                // No powerups, do some idle movement
                if (_movementSmoothingTimer <= 0)
                {
                    if (Math.Abs(_game.Paddle.Left) > _game.Paddle.Width && _random.NextDouble() < 0.2)
                    {
                        _targetX = _game.Paddle.Left > 0 ? -_game.Paddle.Width : _game.Paddle.Width;
                        MovePaddleTowardTarget();
                    }
                    else if (_isMoving && _random.NextDouble() < 0.3)
                    {
                        StopMovement();
                    }
                    _movementSmoothingTimer = _movementSmoothingTime;
                }
            }
        }

        /// <summary>
        /// Handles random paddle movement during idle periods (before serving)
        /// </summary>
        private void IdleWandering(float deltaTime)
        {
            _idleWanderTimer -= deltaTime;

            if (_idleWanderTimer <= 0)
            {
                _idleWanderTimer = _idleWanderInterval;
                _idleWanderInterval = 0.3f + _random.NextSingle() * 0.8f; // Random interval

                // Choose random movement or stop
                float decision = (float)_random.NextDouble();

                if (decision < 0.3f) // 30% chance to move left
                {
                    StopMovement();
                    _game.ApplyGameKey(GameKey.Left);
                    _lastMovementKey = GameKey.Left;
                    _isMoving = true;
                }
                else if (decision < 0.6f) // 30% chance to move right
                {
                    StopMovement();
                    _game.ApplyGameKey(GameKey.Right);
                    _lastMovementKey = GameKey.Right;
                    _isMoving = true;
                }
                else // 40% chance to stop
                {
                    StopMovement();
                }

                // Prevent paddle from going too far to edges while idle
                if (Math.Abs(_game.Paddle.Left) > _game.Width / 3f)
                {
                    // If too far right, move left
                    if (_game.Paddle.Left > 0)
                    {
                        StopMovement();
                        _game.ApplyGameKey(GameKey.Left);
                        _lastMovementKey = GameKey.Left;
                        _isMoving = true;
                    }
                    // If too far left, move right
                    else
                    {
                        StopMovement();
                        _game.ApplyGameKey(GameKey.Right);
                        _lastMovementKey = GameKey.Right;
                        _isMoving = true;
                    }
                }
            }
        }

        /// <summary>
        /// Moves paddle toward the calculated target position by applying appropriate keys
        /// </summary>
        private void MovePaddleTowardTarget()
        {
            if (_movementSmoothingTimer > 0)
                return;

            _movementSmoothingTimer = _movementSmoothingTime / 2; // Faster decision-making when chasing ball

            var distanceToTarget = _targetX - _game.Paddle.Left;

            // Add some "slop" - don't move for tiny differences (human-like behavior)
            var deadzone = _game.Paddle.Width * 0.15f;

            if (MathF.Abs((float)distanceToTarget) < deadzone)
            {
                // Close enough, stop movement
                if (_isMoving)
                {
                    StopMovement();
                }
                return;
            }

            // Move in the appropriate direction, but only send new command if direction changes
            if (distanceToTarget < 0)
            {
                if (_lastMovementKey != GameKey.Left)
                {
                    _game.ApplyGameKey(GameKey.Left);
                    _lastMovementKey = GameKey.Left;
                    _isMoving = true;
                }
            }
            else
            {
                if (_lastMovementKey != GameKey.Right)
                {
                    _game.ApplyGameKey(GameKey.Right);
                    _lastMovementKey = GameKey.Right;
                    _isMoving = true;
                }
            }
        }

        /// <summary>
        /// Stops paddle movement and updates state
        /// </summary>
        private void StopMovement()
        {
            _game.ApplyGameKey(GameKey.Stop);
            _lastMovementKey = GameKey.Stop;
            _isMoving = false;
        }

        private float GetRandomReactionTime()
        {
            return _random.NextSingle() * (_reactionTimeMax - _reactionTimeMin) + _reactionTimeMin;
        }

        /// <summary>
        /// Checks for falling powerups and decides whether to collect them
        /// </summary>
        private bool CheckForPowerupsToCollect()
        {
            PowerUpSprite targetPowerUp = null;
            double closestDistance = double.MaxValue;

            // Find ANY powerup that can be reached
            foreach (var view in _game.GameField.Views)
            {
                if (view is PowerUpSprite powerup && powerup.IsActive)
                {
                    var distance = Math.Abs(powerup.Left - _game.Paddle.Left);
                    var timeToReachPaddle = (powerup.Top - _game.Paddle.Top) / PowerUpSprite.FallSpeed;
                    
                    // Go for ANY powerup that's reachable - MUCH MORE AGGRESSIVE
                    if (timeToReachPaddle > 0.1f && distance < closestDistance)
                    {
                        closestDistance = distance;
                        targetPowerUp = powerup;
                    }
                }
            }

            // If we found any powerup, go for it aggressively
            if (targetPowerUp != null)
            {
                _targetX = targetPowerUp.Left;
                MovePaddleTowardTarget();
                return true; // Signal that we're going for a powerup
            }

            return false; // No powerups to collect
        }


    }
}
