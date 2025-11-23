using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace GameEngine
{
    public class CollisionManager
    {
        private List<Collider> colliders = new List<Collider>();
        public void Register(Collider c)
        {
            colliders.Add(c);
        }
        public void Unregister(Collider c)
        {
            colliders.Remove(c);
        }

        // Query for all colliders intersecting the given bounds
        public List<Collider> QueryIntersections(Bounds b)
        {
            var results = new List<Collider>();
            foreach (var c in colliders)
            {
                var worldBounds = c.GetWorldBounds();
                if (b.Intersects(worldBounds)) results.Add(c);

            }
            return results;

        }

        public static Vector3 ComputeAABBMTV(Bounds a, Bounds b)
        {
            if (!a.Intersects(b)) return Vector3.Zero; // no intersection

            // overlap per axis
            float overlapX = Math.Min(a.Max.X, b.Max.X) - Math.Max(a.Min.X, b.Min.X);
            float overlapY = Math.Min(a.Max.Y, b.Max.Y) - Math.Max(a.Min.Y, b.Min.Y);
            float overlapZ = Math.Min(a.Max.Z, b.Max.Z) - Math.Max(a.Min.Z, b.Min.Z);

            // find smallest overlap axis
            if (overlapX <= overlapY && overlapX <= overlapZ)
            {
                // push along X
                float direction = (a.Center.X < b.Center.X) ? -1f : 1f;
                return new Vector3(overlapX * direction, 0, 0);
            }
            else if (overlapY <= overlapX && overlapY <= overlapZ)
            {
                float direction = (a.Center.Y < b.Center.Y) ? -1f : 1f;
                return new Vector3(0, overlapY * direction, 0);
            }
            else
            {
                float direction = (a.Center.Z < b.Center.Z) ? -1f : 1f;
                return new Vector3(0, 0, overlapZ * direction);
            }
        }
    }
}
