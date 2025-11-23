// Camera.cs
using OpenTK.Mathematics;

namespace GameEngine
{
    public class Camera
    {
        public Vector3 Position;
        public float Pitch;
        public float Yaw;

        public float Fov = MathHelper.DegreesToRadians(65f);
        public float AspectRatio;
        public float Near = 0.1f;
        public float Far = 100f;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
            Pitch = 0f;
            Yaw = -90f; // face forward
        }

        public Vector3 Forward =>
            new Vector3(
                MathF.Cos(MathHelper.DegreesToRadians(Yaw)) *
                MathF.Cos(MathHelper.DegreesToRadians(Pitch)),
                MathF.Sin(MathHelper.DegreesToRadians(Pitch)),
                MathF.Sin(MathHelper.DegreesToRadians(Yaw)) *
                MathF.Cos(MathHelper.DegreesToRadians(Pitch))
            ).Normalized();

        public Vector3 Right =>
            Vector3.Cross(Forward, Vector3.UnitY).Normalized();

        public Vector3 Up =>
            Vector3.Cross(Right, Forward).Normalized();

        public Matrix4 GetViewMatrix() =>
            Matrix4.LookAt(Position, Position + Forward, Up);

        public Matrix4 GetProjectionMatrix() =>
            Matrix4.CreatePerspectiveFieldOfView(Fov, AspectRatio, Near, Far);

        public void AddRotation(float dx, float dy)
        {
            float sensitivity = 0.1f;

            Yaw += dx * sensitivity;
            Pitch -= dy * sensitivity;

            Pitch = MathHelper.Clamp(Pitch, -89f, 89f);
        }
    }
}
