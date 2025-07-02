# Breakout Game Collision Detection Systems

This document explains the two collision detection systems implemented in the Breakout game and how to switch between them.

## Overview

The game now supports two different collision detection approaches:

1. **AABB (Axis-Aligned Bounding Box) Collision Detection** - Traditional method
2. **Raycast Collision Detection** - Advanced predictive method

## Collision Detection Systems

### 1. AABB Collision Detection (Default)

**How it works:**
- Uses rectangle intersection tests (`IntersectsWith`)
- Detects collisions after they have already occurred (penetration-based)
- Calculates overlap areas and determines collision faces based on minimum penetration
- Moves objects out of collision after detection

**Advantages:**
- Simple and fast
- Well-tested and stable
- Good for most gameplay scenarios

**Disadvantages:**
- Can miss fast-moving collisions (tunneling effect)
- Less precise for corner hits
- Collision response happens after penetration

### 2. Raycast Collision Detection (Alternative)

**How it works:**
- Casts rays from the ball's current position in its movement direction
- Predicts collisions before they occur
- Uses multiple rays (center + sides) to simulate ball width
- Calculates exact collision points and normals
- Moves ball to collision point and reflects direction

**Advantages:**
- No tunneling - catches fast-moving collisions
- More precise collision detection
- Better physics simulation with exact collision points
- Proper reflection calculations using surface normals

**Disadvantages:**
- More computationally expensive
- More complex implementation
- May feel different from traditional breakout physics

## How to Switch Between Systems

### Method 1: Compile-time Flag
```csharp
// In BreakoutGame.cs, line ~35
public static bool USE_RAYCAST_COLLISION = false; // AABB (default)
public static bool USE_RAYCAST_COLLISION = true;  // Raycast
```

### Method 2: Runtime Toggle
- Press the **R** key during gameplay to toggle between systems
- The current system is displayed in the score area (e.g., "SCORE: 150 | RAYCAST")
- Debug output shows the switch: "Collision system switched to: RAYCAST"

## Implementation Details

### Key Files
- `Game\BreakoutGame.cs` - Main game logic with collision system switching
- `Game\Internals\RaycastCollision.cs` - Raycast collision implementation
- `Game\BreakoutGameExtensions.cs` - AABB collision utilities

### Key Methods

**AABB System:**
- `ballRect.IntersectsWith(target.HitBox, out var overlap)`
- `overlap.GetCollisionFace(targetRect)`
- Penetration-based collision response

**Raycast System:**
- `DetectCollisionsWithRaycast(ball, deltaSeconds)`
- `RaycastCollision.CastRay(position, direction, distance, radius, targets)`
- `RaycastCollision.CheckWallCollision(...)`
- **Uses traditional collision response**: After detecting collision with raycast, uses the same `CollideBallAndBrick()` and wall collision methods as AABB system for consistent behavior

## Performance Considerations

- **AABB**: O(n) where n is the number of active objects
- **Raycast**: O(n) but with higher constant factor due to ray-rectangle intersection tests
- Both systems use object pooling and efficient hit box caching

## Testing and Comparison

To compare the systems:

1. Start the game with AABB collision (default)
2. Play for a while and note the collision behavior
3. Press **R** to switch to raycast collision
4. Compare the feel and accuracy of collisions
5. Test with fast ball speeds to see tunneling differences

## Debugging

- Current collision system is shown in the score display
- Console output shows system switches
- Both systems use the same sound effects and game logic
- Raycast system provides more detailed collision information for debugging

## Implementation Notes

### Hybrid Approach
The raycast system uses a **hybrid approach** for best results:
1. **Detection**: Uses raycasting for accurate collision detection (no tunneling)
2. **Response**: Uses traditional collision response methods for consistent game feel

This ensures that:
- Collisions are detected accurately even at high speeds
- Ball behavior remains identical between both systems
- Game logic (scoring, brick destruction, etc.) is exactly the same
- Physics feel consistent regardless of collision system

### Key Fix Applied
The initial raycast implementation had issues where balls would disappear because:
- It was using vector-based reflection instead of angle-based reflection
- It was positioning the ball at collision points instead of using traditional offset methods
- Post-collision logic was different from the traditional system

The fix ensures both systems use identical post-collision behavior.

## Future Enhancements

Potential improvements for the raycast system:
- Continuous collision detection for multiple objects
- Swept sphere collision for more accurate ball physics
- Adaptive ray count based on ball speed
- Performance optimizations with spatial partitioning
