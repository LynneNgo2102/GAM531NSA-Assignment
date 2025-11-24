using OpenTK.Mathematics;

namespace BackRoomMap
{
    public class GameObject
    {
        public Transform Transform = new Transform();
        public Mesh Mesh;
        public Texture Texture;
        public AABB Collider;

        public Matrix4 GetModelMatrix()
        {
            return Matrix4.CreateScale(Transform.Scale) *
                   Matrix4.CreateFromQuaternion(Transform.Rotation) *
                   Matrix4.CreateTranslation(Transform.Position);
        }
    }
}
