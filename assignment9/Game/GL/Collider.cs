using OpenTK.Mathematics;

namespace BackRoomMap
{
    // Base class if you add spheres or OBB later
    public abstract class Collider
    {
        public abstract bool Intersects(Collider other);
    }

    // AABB collider (Axis-Aligned Bounding Box)
    public class AABB : Collider
    {
        public Vector3 Center;
        public Vector3 HalfSize;

        public AABB(Vector3 center, Vector3 halfSize)
        {
            Center = center;
            HalfSize = halfSize;
        }

        public Vector3 Min => Center - HalfSize;
        public Vector3 Max => Center + HalfSize;

        public override bool Intersects(Collider other)
        {
            if (other is not AABB b) return false;

            return (Min.X <= b.Max.X && Max.X >= b.Min.X) &&
                   (Min.Y <= b.Max.Y && Max.Y >= b.Min.Y) &&
                   (Min.Z <= b.Max.Z && Max.Z >= b.Min.Z);
        }

        public bool IntersectsPoint(Vector3 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y &&
                   point.Z >= Min.Z && point.Z <= Max.Z;
        }
    }
}
