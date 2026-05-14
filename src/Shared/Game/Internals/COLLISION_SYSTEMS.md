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

### 2. Raycast Collision Detection 

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
