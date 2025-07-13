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

            while (GameKeysQueue.Count > 0)
            {
                ApplyGameKey(GameKeysQueue.Dequeue());
            }

            if ((State == GameState.DemoPlay || State == GameState.Playing) && levelReady)
            {
                if (CheckStateChanged())
                {
                    PlaySound(Sound.Start);
                }

                // get the current player hit box
                Ball.UpdateState(LastFrameTimeNanos);
                var ballRect = Ball.HitBox;

                bool ballCollided = false;
                int bricksChecked = 0;

                // collision detection
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

                                // Count bricks for level completion check
                                foreach (var view in GameField.Views)
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
                                    foreach (var view in GameField.Views)
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
                }

                if ((State == GameState.Playing || State == GameState.DemoPlay) &&
                    (bricksChecked < 1 || BricksLeft == 0))
                {
                    _levelCompletionPending++;

                    //make sure we show some frames after the final collision was detected
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
                    // movement control
                    if (_moveLeft)
                    {
                        UpdatePlayerPosition(Paddle.Left - PADDLE_SPEED * cappedDelta);
                    }

                    if (_moveRight)
                    {
                        UpdatePlayerPosition(Paddle.Left + PADDLE_SPEED * cappedDelta);
                    }
                }
            }

            ProcessSpritesToBeRemoved();

            if (_spritesToBeAdded.Count > 0)
            {
                foreach (var add in _spritesToBeAdded)
                {
                    GameField.AddSubView(add);
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
            PauseGame();
            StopLoop();
        }

        /// <summary>
        /// Engine was paused maybe app went to foreground
        /// </summary>
        protected override void OnResumed()
        {
            if (State == GameState.Playing || State == GameState.DemoPlay)
            {
                StartLoop();
            }
        }

        private int _levelCompletionPending = 0;

        #endregion
    }
}