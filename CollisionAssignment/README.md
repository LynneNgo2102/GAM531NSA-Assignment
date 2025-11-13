What is collision detection and why use simplified shapes?
Collision detection is the process of determining when two game objects intersect or touch so the game can respond (bounce, stop, take damage, destroy, etc.). Exact mesh-based collision checks are expensive and often unnecessary — most 2D games approximate objects with simple bounding shapes (rectangles / circles) to make collision checks extremely fast, robust, and easy to reason about. Simplified shapes reduce CPU cost, avoid tricky floating-point geometry computations, and are enough for convincing gameplay. 
learnopengl.com

AABB–AABB vs AABB–Circle (difference, short explanation)

AABB–AABB: both objects have axis-aligned rectangles (no rotation). Collision is tested by checking overlap on each axis: if the projections on x overlap and the projections on y overlap → collision.

AABB–Circle: one object is a circle and the other is an axis-aligned rectangle. You find the closest point on the rectangle to the circle center (using clamp), then measure the distance from that closest point to the circle center — if the distance ≤ radius the circle intersects the rectangle. This avoids iterating edges or checking angles. 
learnopengl.com

Clamp (what it does and why it’s important)
clamp(value, min, max) returns min if value < min, max if value > max, otherwise value. In AABB–Circle detection it clamps the circle center coordinates to the rectangle’s min/max on each axis to obtain the closest point on the rectangle to the circle center. That closest point is used to compute the distance to the circle center — if that distance is ≤ radius, the circle and rectangle overlap. clamp is the key step that projects the circle center onto the rectangle