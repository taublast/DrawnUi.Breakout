using System.Diagnostics;
using System.Numerics;

namespace Breakout.Game
{
    public partial class BreakoutGame : MauiGame
    {
        #region GAME LOOP

        public override void GameLoop(float deltaSeconds)
        {
            base.GameLoop(deltaSeconds);

            float cappedDelta = deltaSeconds;

            GameFieldArea = GameField.GetHitBox();
            BricksArea = BricksContainer.GetHitBox();

            ProcessInput();

            if ((State == GameState.DemoPlay || State == GameState.Playing) && levelReady)
            {
                // Update all active balls
                foreach (var ball in ActiveBalls)
                {
                    ball.UpdateState(LastFrameTimeNanos);
                }

                foreach (var child in BricksContainer.Views)
                {
                    if (child is BrickSprite brick && brick.IsActive)
                    {
                        brick.UpdateState(LastFrameTimeNanos);
                    }
                }

                // collision detection for all active balls
                foreach (var ball in ActiveBalls.ToList())
                {
                    if (!ball.IsActive) continue;

                    bool ballCollided = false;
                    var ballRect = ball.HitBox;

                    //bricks
                    if (ball.IsMoving)
                    {
                        if (USE_RAYCAST_COLLISION)
                        {
                            // Use raycast collision detection
                            ballCollided = DetectCollisionsWithRaycast(ball, cappedDelta);
                        }
                        else
                        {
                            // Use traditional AABB collision detection
                            if (!ballCollided && ball.IsActive)
                            {
                                foreach (var view in GameField.Views)
                                {
                                    if (view == BricksContainer)
                                    {
                                        foreach (var child in BricksContainer.Views)
                                        {
                                            if (child is BrickSprite brick && brick.IsActive)
                                            {
                                                if (ballRect.IntersectsWith(brick.HitBox, out var overlap))
                                                {
                                                    if (ball.IsFireball)
                                                    {
                                                        // Fireball mode: destroy brick but don't bounce
                                                        CollideBallAndBrick(brick, ball, overlap, isFireball: true);
                                                        // Don't set ballCollided = true, so ball continues through
                                                    }
                                                    else
                                                    {
                                                        // Normal mode: destroy brick and bounce
                                                        CollideBallAndBrick(brick, ball, overlap);
                                                        ballCollided = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                        //maybe GameState would change after all of that
                        if (BricksLeftToBreak == 0)
                        {
                            break;
                        }

                        if (State == GameState.Playing || State == GameState.DemoPlay)
                        {
                            //paddle(s) - only for traditional collision detection (raycast handles this in DetectCollisionsWithRaycast)
                            if (!USE_RAYCAST_COLLISION && !ballCollided && ball.IsActive)
                            {
                                if (MathF.Sin(ball.Angle) > 0) //only if ball moves downward
                                {
                                    foreach (var view in GameField.Views)
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
                                    ball.MoveOffset(penetration * 1.1f, 0);
                                    ball.Angle = MathF.PI - ball.Angle;
                                    PlaySound(Sound.Wall, new Vector3(-1.0f, 0f, -1f));
                                    ballCollided = true;
                                }
                                // Right wall
                                else if (ballRect.Right > Width)
                                {
                                    var penetration = ballRect.Right - Width;
                                    ball.MoveOffset(-penetration * 1.1f, 0);
                                    ball.Angle = MathF.PI - ball.Angle;
                                    PlaySound(Sound.Wall, new Vector3(2.0f, 0f, -1f));
                                    ballCollided = true;
                                }

                                // Top wall
                                if (ballRect.Top < 0)
                                {
                                    var penetration = -ballRect.Top;
                                    ball.MoveOffset(0, penetration * 1.1f);
                                    ball.Angle = -ball.Angle;
                                    PlaySound(Sound.Wall);
                                    ballCollided = true;
                                }

                                if (ballRect.Bottom > GameField.Height)
                                {
                                    ballCollided = true;
                                    PlaySound(Sound.Oops);
                                    if (CHEAT_INVULNERABLE)
                                    {
                                        // In cheat mode, just reset the ball position
                                        ball.Top = GameField.Height - ball.Height - 10;
                                        ball.Angle = -ball.Angle;
                                    }
                                    else
                                    {
                                        // Remove this ball from play
                                        RemoveBall(ball);

                                        // Check if all balls are gone
                                        if (ActiveBalls.Count == 0)
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
                                                    GameLost();
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
                            }

                            // Update ball position - raycast collision handles positioning during collision detection
                            if (ball.IsActive && (!ballCollided || !USE_RAYCAST_COLLISION))
                            {
                                ball.UpdatePosition(cappedDelta);
                            }
                        }
                }

                // Handle powerups separately
                foreach (var x in this.GameField.Views.ToList())
                {
                    if (x is PowerUpSprite powerup && powerup.IsActive)
                    {
                        // Update powerup state and position
                        powerup.UpdateState(LastFrameTimeNanos);
                        powerup.UpdateRotation(cappedDelta);

                        // Check if powerup hit bottom - remove it
                        if (powerup.Top > GameField.Height)
                        {
                            RemoveReusable(powerup);
                        }
                        else
                        {
                            // Check collision with paddle
                            var powerupRect = powerup.HitBox;

                            foreach (var view in GameField.Views)
                            {
                                if (view is PaddleSprite paddle && paddle.IsActive)
                                {
                                    paddle.UpdateState(LastFrameTimeNanos);

                                    if (powerupRect.IntersectsWith(paddle.HitBox))
                                    {
                                        ApplyPowerUp(powerup.Type);
                                        RemoveReusable(powerup);
                                        break;
                                    }
                                }
                            }

                            // Move powerup down if still active
                            if (powerup.IsActive)
                            {
                                powerup.Top += PowerUpSprite.FallSpeed * cappedDelta;
                            }
                        }
                    }
                    else if (x is BulletSprite bullet && bullet.IsActive)
                    {
                        bullet.UpdateState(LastFrameTimeNanos);

                        // Check if bullet reached top - remove it
                        if (bullet.HitBox.Top < 0)
                        {
                            RemoveReusable(bullet);
                        }
                        else
                        {
                            // Check collision with bricks using raycast
                            bool bulletHit = false;

                            if (USE_RAYCAST_COLLISION)
                            {
                                bulletHit = DetectBulletCollisionsWithRaycast(bullet, cappedDelta);
                            }
                            else
                            {
                                // Traditional AABB collision for bullets vs bricks and powerups
                                var bulletRect = bullet.HitBox;

                                foreach (var view in GameField.Views)
                                {
                                    if (view == BricksContainer)
                                    {
                                        foreach (var child in BricksContainer.Views)
                                        {
                                            if (child is BrickSprite brick && brick.IsActive)
                                            {
                                                if (bulletRect.IntersectsWith(brick.HitBox))
                                                {
                                                    CollideBulletAndBrick(bullet, brick);
                                                    bulletHit = true;
                                                    break;
                                                }
                                            }
                                        }

                                        if (bulletHit) break;
                                    }
                                    // Check collision with powerups
                                    else if (view is PowerUpSprite fallingPowerup && fallingPowerup.IsActive)
                                    {
                                        if (bulletRect.IntersectsWith(fallingPowerup.HitBox))
                                        {
                                            RemoveReusable(bullet);
                                            RemoveReusable(fallingPowerup);
                                            bulletHit = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            // Move bullet up if still active
                            if (bullet.IsActive && !bulletHit)
                            {
                                bullet.Top -= BulletSprite.Speed * cappedDelta;
                            }
                        }
                    }
                }

                //check we broke all bricks
                if ((State == GameState.Playing || State == GameState.DemoPlay) &&
                    (BricksLeftToBreak == 0))
                {
                    _levelCompletionPending++;

                    //make sure we show some frames after the final collision was detected
                    if (_levelCompletionPending > 20)
                    {
                        State = GameState.LevelComplete;
                        _levelCompletionPending = 0;
                    }
                }

                //update ai
                if (State == GameState.DemoPlay)
                {
                    AIController.UpdateAI(cappedDelta);
                }

                //update player state and position
                if (State == GameState.Playing || State == GameState.DemoPlay)
                {
                    // Update paddle powerup duration
                    if (Paddle.PowerupDuration > 0)
                    {
                        Paddle.PowerupDuration -= cappedDelta;
                        if (Paddle.PowerupDuration <= 0)
                        {
                            // timer expired
                            ResetPowerUp();
                        }
                    }

                    // movement control
                    if (IsMovingLeft)
                    {
                        UpdatePlayerPosition(Paddle.Left - PADDLE_SPEED * cappedDelta);
                    }

                    if (IsMovingRight)
                    {
                        UpdatePlayerPosition(Paddle.Left + PADDLE_SPEED * cappedDelta);
                    }
                }
            }

            // Ensure all balls never exit game field bounds (raycast collision fallback)
            foreach (var ball in ActiveBalls.ToList())
            {
                if (!ball.IsActive) continue;

                var ballLeftLimit = -GameField.Width / 2f + ball.Width / 2f;
                var ballRightLimit = GameField.Width / 2f - ball.Width / 2f;
                var wasLeft = ball.Left;
                ball.Left = Math.Clamp(ball.Left, ballLeftLimit, ballRightLimit);

                // If ball was clamped horizontally, reflect its horizontal direction
                if (wasLeft != ball.Left && ball.IsMoving)
                {
                    ball.Angle = MathF.PI - ball.Angle; // Reflect horizontally
                    PlaySound(Sound.Wall, new Vector3(ball.Left < 0 ? -1.0f : 1.0f, 0f, -1f));
                }

                var ballTopLimit = -GameField.Height + ball.Height;
                var ballBottomLimit = 0;
                var wasTop = ball.Top;
                ball.Top = Math.Clamp(ball.Top, ballTopLimit, ballBottomLimit);

                // If ball was clamped vertically, reflect its vertical direction with slight randomization
                if (wasTop != ball.Top && ball.IsMoving)
                {
                    // Reflect vertically
                    ball.Angle = -ball.Angle;

                    // Add slight randomization to prevent infinite vertical bouncing
                    var randomOffset = (Random.Shared.NextSingle() - 0.5f) * 0.2f; // ±0.1 radians (~±6 degrees)
                    ball.Angle += randomOffset;

                    // Ensure angle doesn't become too horizontal (use existing clamping)
                    ball.Angle = BallSprite.ClampAngleFromHorizontal(ball.Angle);

                    PlaySound(Sound.Wall, new Vector3(0f, ball.Top < -GameField.Height / 2f ? -1.0f : 1.0f, -1f));
                }
            }
            
            ProcessSpritesToBeRemoved();

            if (_spritesToBeAdded.Count > 0)
            {
                foreach (var add in _spritesToBeAdded)
                {
                    if (add is BrickSprite brick)
                    {
                        BricksContainer.AddSubView(brick);
                    }
                    else
                    {
                        GameField.AddSubView(add);
                    }
                }

                _spritesToBeAdded.Clear();
            }

            if (State == GameState.Playing || State == GameState.DemoPlay)
            {
                levelReady = true; //we have added all sprites, can play now
            }
            else 
            if (State == GameState.LevelComplete)
            {
                LevelComplete();
            }
        }

        /// <summary>
        /// Engine was paused maybe app went to background
        /// </summary>
        protected override void OnPaused()
        {
            if (_audioService != null)
            {
                _audioService.IsMuted = true;
                _audioService.StopBackgroundMusic();
            }
            
            PauseGame();
            
            StopLoop();
        }

        /// <summary>
        /// Engine was paused maybe app went to foreground
        /// </summary>
        protected override void OnResumed()
        {
            if (_audioService != null)
            {
                _audioService.IsMuted = false;
                SetupBackgroundMusic();
            }

            StartLoop();
        }

        private int _levelCompletionPending = 0;
        private bool _levelCompletionPrompt = false;

        #endregion
    }
}