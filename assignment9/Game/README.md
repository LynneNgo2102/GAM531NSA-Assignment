----------------------------------------------------------------------------
Name: Lynne Ngo
Student ID: 123456789
Date: November 24, 2025
Section: NSA
----------------------------------------------------------------------------

The collision detection method you used.

- I used AABB (Axis-Aligned Bounding Box) collision detection.
Each object (pillar, walls) has a box defined by its center and half-size.
The player uses a sphere collider, and collision is checked using sphere–AABB intersection.


How your collision and movement integration works.

- The camera calculates the desired movement for this frame.

- Before applying it, I create a predicted AABB for the player at the new position.

- I check that predicted collider against every scene object’s collider.

- If a collision is detected → movement is canceled (player stays at previous position).

- After that, I apply room boundary limits so the player can’t leave the room.


Any challenges encountered and how you solved them.
- Challenge: Tunneling issue where fast movement caused the player to pass through thin objects.