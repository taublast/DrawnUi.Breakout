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
                // get the current player hit box
                Ball.UpdateState(LastFrameTimeNanos);
                var ballRect = Ball.HitBox;

                foreach (var child in BricksContainer.Views)
                {
                    if (child is BrickSprite brick && brick.IsActive)
                    {
                        brick.UpdateState(LastFrameTimeNanos);
                    }
                }

                // collision detection
                bool ballCollided = false;
                foreach (var x in this.GameField.Views.ToList())
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
                                    Ball.MoveOffset(penetration * 1.1f, 0);
                                    ball.Angle = MathF.PI - ball.Angle;
                                    PlaySound(Sound.Wall, new Vector3(-1.0f, 0f, -1f));
                                    ballCollided = true;
                                }
                                // Right wall
                                else if (ballRect.Right > Width)
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

                                if (ballRect.Bottom > GameField.Height)
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

                            // Update ball position - raycast collision handles positioning during collision detection
                            if (ball.IsActive && (!ballCollided || !USE_RAYCAST_COLLISION))
                            {
                                ball.UpdatePosition(cappedDelta);
                            }
                        }
                    }

                    else if (x is PowerUpSprite powerup && powerup.IsActive)
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

            // Ensure ball never exits game field bounds 
            #region fix raycast
            var ballLeftLimit = -GameField.Width / 2f + Ball.Width / 2f;
            var ballRightLimit = GameField.Width / 2f - Ball.Width / 2f;
            var wasLeft = Ball.Left;
            Ball.Left = Math.Clamp(Ball.Left, ballLeftLimit, ballRightLimit);

            // If ball was clamped horizontally, reflect its horizontal direction
            if (wasLeft != Ball.Left && Ball.IsMoving)
            {
                Ball.Angle = MathF.PI - Ball.Angle; // Reflect horizontally
                PlaySound(Sound.Wall, new Vector3(Ball.Left < 0 ? -1.0f : 1.0f, 0f, -1f));
            }

            var ballTopLimit = -GameField.Height + Ball.Height;
            var ballBottomLimit = 0;
            var wasTop = Ball.Top;
            Ball.Top = Math.Clamp(Ball.Top, ballTopLimit, ballBottomLimit);

            // If ball was clamped vertically, reflect its vertical direction with slight randomization
            if (wasTop != Ball.Top && Ball.IsMoving)
            {
                // Reflect vertically
                Ball.Angle = -Ball.Angle;

                // Add slight randomization to prevent infinite vertical bouncing
                var randomOffset = (Random.Shared.NextSingle() - 0.5f) * 0.2f; // ±0.1 radians (~±6 degrees)
                Ball.Angle += randomOffset;

                // Ensure angle doesn't become too horizontal (use existing clamping)
                Ball.Angle = BallSprite.ClampAngleFromHorizontal(Ball.Angle);

                PlaySound(Sound.Wall, new Vector3(0f, Ball.Top < -GameField.Height / 2f ? -1.0f : 1.0f, -1f));
            }
            #endregion
            
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
            else if (State == GameState.LevelComplete)
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
            }
            if (State == GameState.Playing || State == GameState.DemoPlay)
            {
                StartLoop();
            }
        }

        private int _levelCompletionPending = 0;

        #endregion
    }
}