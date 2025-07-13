﻿using System.Numerics;
using SkiaSharp;

namespace Breakout.Game
{
    /// <summary>
    /// Implements raycasting collision detection for more accurate collision handling
    /// </summary>
    public static class RaycastCollision
    {
        // Minimum threshold for direction components to avoid division by very small numbers
        private const float MIN_DIRECTION_THRESHOLD = 0.0001f;

        /// <summary>
        /// Represents the result of a raycast collision
        /// </summary>
        public struct RaycastHit
        {
            public bool Collided { get; set; }
            public float Distance { get; set; }
            public Vector2 Point { get; set; }
            public Vector2 Normal { get; set; }
            public IWithHitBox Target { get; set; }
            public CollisionFace Face { get; set; }

            public static RaycastHit None => new RaycastHit { Collided = false };
        }

        /// <summary>
        /// Performs a raycast from the ball's current position in its movement direction
        /// </summary>
        /// <param name="origin">Current position of the ball</param>
        /// <param name="direction">Direction vector of ball's movement</param>
        /// <param name="distance">Distance to check for collision (based on ball's speed and deltaTime)</param>
        /// <param name="radius">Radius of the ball</param>
        /// <param name="targets">Collection of collision targets</param>
        /// <returns>Information about the collision if it occurred</returns>
        public static RaycastHit CastRay(Vector2 origin, Vector2 direction, float distance, float radius,
            IEnumerable<IWithHitBox> targets)
        {
            var closestHit = RaycastHit.None;
            closestHit.Distance = float.MaxValue;

            // Create multiple raycasts to simulate the ball's width
            // Main center ray
            var centerHit = CastSingleRay(origin, direction, distance, radius, targets);
            if (centerHit.Collided && centerHit.Distance < closestHit.Distance)
            {
                closestHit = centerHit;
            }

            // Calculate perpendicular vector to the direction for side rays
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);
            perpendicular = Vector2.Normalize(perpendicular) * radius * 0.8f; // 80% of radius for the side rays

            // Left side ray
            Vector2 leftOrigin = origin + perpendicular;
            var leftHit = CastSingleRay(leftOrigin, direction, distance, radius * 0.2f, targets);
            if (leftHit.Collided && leftHit.Distance < closestHit.Distance)
            {
                closestHit = leftHit;
            }

            // Right side ray
            Vector2 rightOrigin = origin - perpendicular;
            var rightHit = CastSingleRay(rightOrigin, direction, distance, radius * 0.2f, targets);
            if (rightHit.Collided && rightHit.Distance < closestHit.Distance)
            {
                closestHit = rightHit;
            }

            return closestHit.Collided ? closestHit : RaycastHit.None;
        }

        /// <summary>
        /// Casts a single ray against all targets
        /// </summary>
        private static RaycastHit CastSingleRay(Vector2 origin, Vector2 direction, float distance, float radius,
            IEnumerable<IWithHitBox> targets)
        {
            var closestHit = RaycastHit.None;
            closestHit.Distance = float.MaxValue;

            foreach (var target in targets)
            {
                if (target is BallSprite)
                    continue;

                var hit = TestRaycastAgainstRect(origin, direction, distance, radius, target.HitBox, target);

                if (hit.Collided && hit.Distance < closestHit.Distance)
                {
                    closestHit = hit;
                }
            }

            return closestHit.Collided ? closestHit : RaycastHit.None;
        }

        /// <summary>
        /// Tests if a ray collides with a rectangle
        /// </summary>
        private static RaycastHit TestRaycastAgainstRect(Vector2 origin, Vector2 direction, float maxDistance,
            float radius, SKRect rect, IWithHitBox target)
        {
            // Make sure we're using the correct HitBox from the target
            rect = target.HitBox;

            // Expand the rect by the ball's radius to account for the ball's size
            var expandedRect = new SKRect(
                rect.Left - radius,
                rect.Top - radius,
                rect.Right + radius,
                rect.Bottom + radius
            );

            // Points representing the expanded rectangle
            Vector2 rectMin = new Vector2(expandedRect.Left, expandedRect.Top);
            Vector2 rectMax = new Vector2(expandedRect.Right, expandedRect.Bottom);

            // Check if the origin is inside the expanded rectangle - this would cause negative distance
            bool originInsideRect = origin.X >= rectMin.X && origin.X <= rectMax.X &&
                                   origin.Y >= rectMin.Y && origin.Y <= rectMax.Y;

            if (originInsideRect)
            {
                // Return no collision if we're already inside - prevents the stuck bug
                return RaycastHit.None;
            }

            // Calculate distance to intersection points
            float tNear = float.NegativeInfinity;
            float tFar = float.PositiveInfinity;

            // Normal of the hit face
            Vector2 hitNormal = Vector2.Zero;
            CollisionFace hitFace = CollisionFace.None;

            // Check X axis intersection - FIXED threshold for shallow angles
            if (Math.Abs(direction.X) < MIN_DIRECTION_THRESHOLD)
            {
                // Ray is nearly parallel to X axis
                if (origin.X < rectMin.X || origin.X > rectMax.X)
                    return RaycastHit.None;
            }
            else
            {
                float tx1 = (rectMin.X - origin.X) / direction.X;
                float tx2 = (rectMax.X - origin.X) / direction.X;

                if (tx1 > tx2)
                {
                    float temp = tx1;
                    tx1 = tx2;
                    tx2 = temp;
                }

                tNear = Math.Max(tNear, tx1);
                tFar = Math.Min(tFar, tx2);

                if (tNear == tx1)
                {
                    hitNormal = new Vector2(-1, 0);
                    hitFace = CollisionFace.Left;
                }
                else if (tNear == tx2)
                {
                    hitNormal = new Vector2(1, 0);
                    hitFace = CollisionFace.Right;
                }

                if (tNear > tFar || tFar < 0)
                    return RaycastHit.None;
            }

            // Check Y axis intersection - FIXED threshold for shallow angles
            if (Math.Abs(direction.Y) < MIN_DIRECTION_THRESHOLD)
            {
                // Ray is nearly parallel to Y axis
                if (origin.Y < rectMin.Y || origin.Y > rectMax.Y)
                    return RaycastHit.None;
            }
            else
            {
                float ty1 = (rectMin.Y - origin.Y) / direction.Y;
                float ty2 = (rectMax.Y - origin.Y) / direction.Y;

                if (ty1 > ty2)
                {
                    float temp = ty1;
                    ty1 = ty2;
                    ty2 = temp;
                }

                float originalTNear = tNear;

                tNear = Math.Max(tNear, ty1);
                tFar = Math.Min(tFar, ty2);

                if (tNear > originalTNear)
                {
                    if (tNear == ty1)
                    {
                        hitNormal = new Vector2(0, -1);
                        hitFace = CollisionFace.Top;
                    }
                    else
                    {
                        hitNormal = new Vector2(0, 1);
                        hitFace = CollisionFace.Bottom;
                    }
                }

                if (tNear > tFar || tFar < 0)
                    return RaycastHit.None;
            }

            // Ensure we only detect collisions at positive distances
            if (tNear < 0 || tNear > maxDistance)
                return RaycastHit.None;

            // Calculate the hit point
            Vector2 hitPoint = origin + direction * tNear;

            return new RaycastHit
            {
                Collided = true,
                Distance = tNear,
                Point = hitPoint,
                Normal = hitNormal,
                Target = target,
                Face = hitFace
            };
        }

        /// <summary>
        /// Checks if a moving object (ball) would collide with walls using proper distance calculations
        /// </summary>
        public static RaycastHit CheckWallCollision(Vector2 position, Vector2 direction, float radius,
            float maxDistance, SKRect gameField)
        {
            RaycastHit closestHit = RaycastHit.None;
            closestHit.Distance = float.MaxValue;

            // Normalize direction to unit vector for distance calculations
            Vector2 normalizedDirection = Vector2.Normalize(direction);

            // Left wall collision
            if (normalizedDirection.X < -MIN_DIRECTION_THRESHOLD) // Moving left
            {
                float wallPosition = gameField.Left + radius; // Ball edge should not go below this X position
                float distanceToWall = position.X - wallPosition;

                if (distanceToWall > 0) // Not already past the wall
                {
                    // Calculate actual distance along the ray to reach the wall
                    float rayDistance = distanceToWall / -normalizedDirection.X;

                    if (rayDistance >= 0 && rayDistance <= maxDistance && rayDistance < closestHit.Distance)
                    {
                        Vector2 hitPoint = position + normalizedDirection * rayDistance;

                        // Verify Y coordinate is within bounds
                        if (hitPoint.Y >= gameField.Top + radius && hitPoint.Y <= gameField.Bottom - radius)
                        {
                            closestHit.Collided = true;
                            closestHit.Distance = rayDistance;
                            closestHit.Normal = new Vector2(1, 0);
                            closestHit.Face = CollisionFace.Left;
                            closestHit.Point = new Vector2(gameField.Left, hitPoint.Y);
                        }
                    }
                }
            }

            // Right wall collision
            if (normalizedDirection.X > MIN_DIRECTION_THRESHOLD) // Moving right
            {
                float wallPosition = gameField.Right - radius; // Ball edge should not go above this X position
                float distanceToWall = wallPosition - position.X;

                if (distanceToWall > 0) // Not already past the wall
                {
                    float rayDistance = distanceToWall / normalizedDirection.X;

                    if (rayDistance >= 0 && rayDistance <= maxDistance && rayDistance < closestHit.Distance)
                    {
                        Vector2 hitPoint = position + normalizedDirection * rayDistance;

                        if (hitPoint.Y >= gameField.Top + radius && hitPoint.Y <= gameField.Bottom - radius)
                        {
                            closestHit.Collided = true;
                            closestHit.Distance = rayDistance;
                            closestHit.Normal = new Vector2(-1, 0);
                            closestHit.Face = CollisionFace.Right;
                            closestHit.Point = new Vector2(gameField.Right, hitPoint.Y);
                        }
                    }
                }
            }

            // Top wall collision (ball hits top of screen - should bounce)
            if (normalizedDirection.Y < -MIN_DIRECTION_THRESHOLD) // Moving up
            {
                float wallPosition = gameField.Top + radius; // Ball edge should not go above this Y position
                float distanceToWall = position.Y - wallPosition;

                if (distanceToWall > 0) // Not already past the wall
                {
                    float rayDistance = distanceToWall / -normalizedDirection.Y;

                    if (rayDistance >= 0 && rayDistance <= maxDistance && rayDistance < closestHit.Distance)
                    {
                        Vector2 hitPoint = position + normalizedDirection * rayDistance;

                        if (hitPoint.X >= gameField.Left + radius && hitPoint.X <= gameField.Right - radius)
                        {
                            closestHit.Collided = true;
                            closestHit.Distance = rayDistance;
                            closestHit.Normal = new Vector2(0, 1);
                            closestHit.Face = CollisionFace.Bottom; // Ball hits bottom face of top wall
                            closestHit.Point = new Vector2(hitPoint.X, gameField.Top);
                        }
                    }
                }
            }

            // Bottom wall collision (ball hits bottom of screen - should lose life)
            if (normalizedDirection.Y > MIN_DIRECTION_THRESHOLD) // Moving down
            {
                float wallPosition = gameField.Bottom - radius; // Ball edge should not go below this Y position
                float distanceToWall = wallPosition - position.Y;

                if (distanceToWall > 0) // Not already past the wall
                {
                    float rayDistance = distanceToWall / normalizedDirection.Y;

                    if (rayDistance >= 0 && rayDistance <= maxDistance && rayDistance < closestHit.Distance)
                    {
                        Vector2 hitPoint = position + normalizedDirection * rayDistance;

                        if (hitPoint.X >= gameField.Left + radius && hitPoint.X <= gameField.Right - radius)
                        {
                            closestHit.Collided = true;
                            closestHit.Distance = rayDistance;
                            closestHit.Normal = new Vector2(0, -1);
                            closestHit.Face = CollisionFace.Top; // Ball hits top face of bottom wall
                            closestHit.Point = new Vector2(hitPoint.X, gameField.Bottom);
                        }
                    }
                }
            }

            return closestHit;
        }

        /// <summary>
        /// Reflects a vector off a surface with the given normal
        /// </summary>
        public static Vector2 Reflect(Vector2 direction, Vector2 normal)
        {
            return direction - 2 * Vector2.Dot(direction, normal) * normal;
        }
    }
}