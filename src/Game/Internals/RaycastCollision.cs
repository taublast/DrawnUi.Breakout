using SkiaSharp;
using System.Numerics;

namespace BreakoutGame.Game
{
    /// <summary>
    /// Implements raycasting collision detection for more accurate collision handling
    /// </summary>
    public static class RaycastCollision
    {
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

            // Check X axis intersection
            if (Math.Abs(direction.X) < 0.0001f)
            {
                // Ray is parallel to X axis
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

            // Check Y axis intersection
            if (Math.Abs(direction.Y) < 0.0001f)
            {
                // Ray is parallel to Y axis
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
        /// Checks if a moving object (ball) would collide with walls
        /// </summary>
        public static RaycastHit CheckWallCollision(Vector2 position, Vector2 direction, float radius,
            float distance, float screenWidth, float screenHeight)
        {
            RaycastHit hit = RaycastHit.None;
            hit.Distance = float.MaxValue;

            // Left wall
            if (direction.X < 0)
            {
                float distanceToWall = position.X - radius;
                float timeToHit = distanceToWall / -direction.X;

                if (timeToHit >= 0 && timeToHit <= distance && timeToHit < hit.Distance)
                {
                    hit.Collided = true;
                    hit.Distance = timeToHit;
                    hit.Normal = new Vector2(1, 0);
                    hit.Face = CollisionFace.Right;
                    hit.Point = new Vector2(0, position.Y + direction.Y * timeToHit);
                }
            }

            // Right wall
            if (direction.X > 0)
            {
                float distanceToWall = screenWidth - position.X - radius;
                float timeToHit = distanceToWall / direction.X;

                if (timeToHit >= 0 && timeToHit <= distance && timeToHit < hit.Distance)
                {
                    hit.Collided = true;
                    hit.Distance = timeToHit;
                    hit.Normal = new Vector2(-1, 0);
                    hit.Face = CollisionFace.Left;
                    hit.Point = new Vector2(screenWidth, position.Y + direction.Y * timeToHit);
                }
            }

            // Top wall
            if (direction.Y < 0)
            {
                float distanceToWall = position.Y - radius;
                float timeToHit = distanceToWall / -direction.Y;

                if (timeToHit >= 0 && timeToHit <= distance && timeToHit < hit.Distance)
                {
                    hit.Collided = true;
                    hit.Distance = timeToHit;
                    hit.Normal = new Vector2(0, 1);
                    hit.Face = CollisionFace.Bottom;
                    hit.Point = new Vector2(position.X + direction.X * timeToHit, 0);
                }
            }

            // Bottom wall (maybe don't check this to let the ball fall out)
            if (direction.Y > 0)
            {
                float distanceToWall = screenHeight - position.Y - radius;
                float timeToHit = distanceToWall / direction.Y;

                if (timeToHit >= 0 && timeToHit <= distance && timeToHit < hit.Distance)
                {
                    hit.Collided = true;
                    hit.Distance = timeToHit;
                    hit.Normal = new Vector2(0, -1);
                    hit.Face = CollisionFace.Top;
                    hit.Point = new Vector2(position.X + direction.X * timeToHit, screenHeight);
                }
            }

            return hit;
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