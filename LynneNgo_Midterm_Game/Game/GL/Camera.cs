using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;


namespace EndlessHallway
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 Front = -Vector3.UnitZ;
        public Vector3 Up = Vector3.UnitY;
        Vector3 Right = Vector3.UnitX;
        float yaw = -90f, pitch = 0f;
        public float Speed = 5f;
        public float Sensitivity = 0.2f;

        public Camera(Vector3 pos) { Position = pos; UpdateVectors(); }

        public Matrix4 GetViewMatrix() => Matrix4.LookAt(Position, Position + Front, Up);

        public void ProcessMouseDelta(double dx, double dy)
        {
            yaw += (float)dx * Sensitivity;
            pitch -= (float)dy * Sensitivity;
            pitch = MathHelper.Clamp(pitch, -89f, 89f);
            UpdateVectors();
        }

        void UpdateVectors()
        {
            Vector3 f;
            f.X = MathF.Cos(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            f.Y = MathF.Sin(MathHelper.DegreesToRadians(pitch));
            f.Z = MathF.Sin(MathHelper.DegreesToRadians(yaw)) * MathF.Cos(MathHelper.DegreesToRadians(pitch));
            Front = Vector3.Normalize(f);
            Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
            Up = Vector3.Normalize(Vector3.Cross(Right, Front));
        }

        public void ProcessKeyboard(KeyboardState keys, float delta)
        {
            var vel = Speed * delta;
            if (keys.IsKeyDown(Keys.W)) Position += Front * vel;
            if (keys.IsKeyDown(Keys.S)) Position -= Front * vel;
            if (keys.IsKeyDown(Keys.A)) Position -= Right * vel;
            if (keys.IsKeyDown(Keys.D)) Position += Right * vel;
        }
    }
}