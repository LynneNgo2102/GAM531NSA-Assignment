using assignment9.src.Engine;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;

namespace GameEngine
{
    public class PlayerController
    {
        public Transform Transform;
        public AABBCollider Collider;
        public float Speed = 4.0f;
        public CollisionManager CollisionMgr;
        public Vector3 Velocity = Vector3.Zero;
        public float PlayerHeight = 1.8f;

        public PlayerController(Transform t, CollisionManager cm)
        {
            Transform = t;
            CollisionMgr = cm;
            var halfExtents = new Vector3(0.35f, PlayerHeight / 2f, 0.35f);
            Collider = new AABBCollider(t, Bounds.FromCenterExtents(Vector3.Zero, halfExtents));
            cm.Register(Collider);
        }

        // Call once per frame: deltaSeconds, keyboard state
        public void Update(double dt, KeyboardState kb, Camera cam)
        {
            Vector3 input = Vector3.Zero;

            // Example: WASD movement relative to camera
            if (kb.IsKeyDown(Keys.W)) input += cam.Forward;
            if (kb.IsKeyDown(Keys.S)) input -= cam.Forward;
            if (kb.IsKeyDown(Keys.A)) input -= cam.Right;
            if (kb.IsKeyDown(Keys.D)) input += cam.Right;

            input.Y = 0;
            if (input.LengthSquared > 0.0001f) input = input.Normalized();

            Vector3 desiredMove = input * Speed * (float)dt;

            // Predict new bounds
            var currentBounds = Collider.GetWorldBounds();
            var predictedBounds = currentBounds.Translate(desiredMove);

            // Query for potential collisions
            List<Collider> hits = CollisionMgr.QueryIntersections(predictedBounds);

            if (hits.Count == 0)
            {
                // no collision predicted -> apply movement
                Transform.Position += desiredMove;
            }
            else
            {
                // Collision predicted -> compute MTV for each hit, accumulate minimal correction
                Vector3 totalCorrection = Vector3.Zero;
                foreach (var hit in hits)
                {
                    if (hit == Collider) continue;
                    var hitBounds = hit.GetWorldBounds();
                    var movedBounds = currentBounds.Translate(desiredMove + totalCorrection);
                    if (!movedBounds.Intersects(hitBounds)) continue;
                    var mtv = CollisionManager.ComputeAABBMTV(movedBounds, hitBounds);
                    // apply only the component that reduces overlap
                    totalCorrection += mtv;
                }

                // sliding: allow movement in remaining axes (apply correction then move remaining)
                // Apply correction first
                Transform.Position += totalCorrection;

                // compute leftover movement after correction
                Vector3 leftover = desiredMove + totalCorrection;
                // Try movement along X
                Vector3 moveX = new Vector3(leftover.X, 0, 0);
                if (!CollisionMgr.QueryIntersections(Collider.GetWorldBounds().Translate(moveX)).Exists(c => c != Collider))
                {
                    Transform.Position += moveX;
                }
                // Try movement along Z
                Vector3 moveZ = new Vector3(0, 0, leftover.Z);
                if (!CollisionMgr.QueryIntersections(Collider.GetWorldBounds().Translate(moveZ)).Exists(c => c != Collider))
                {
                    Transform.Position += moveZ;
                }
            }
        }
    }
}
