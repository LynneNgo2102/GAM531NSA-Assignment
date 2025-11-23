// Game.cs
using assignment9.src.Engine;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;




namespace GameEngine
{
    public class Game : GameWindow
    {
        // Camera
        private Camera _camera;

        // Player movement
        private Vector3 _playerPosition = new Vector3(0, 1.0f, 3f);
        private float _moveSpeed = 3f;

        // Scene objects (with AABB colliders)
        private readonly List<SceneObject> _sceneObjects = new();

        // Timing
        private float _deltaTime;

        public Game(GameWindowSettings gws, NativeWindowSettings nws)
            : base(gws, nws)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);

            // ✔ Valid because Game inherits GameWindow
            CursorGrabbed = true;

            // ✔ Fixed: Your Camera only takes (Vector3 position)
            _camera = new Camera(_playerPosition, Size.X / (float)Size.Y);


            LoadSceneObjects();

            Console.WriteLine("Game Loaded");
        }

        private void LoadSceneObjects()
        {
            // Walls in a square
            _sceneObjects.Add(new SceneObject(new Vector3(0, 0, -5), new Vector3(10, 2, 0.5f)));
            _sceneObjects.Add(new SceneObject(new Vector3(-5, 0, 0), new Vector3(0.5f, 2, 10)));
            _sceneObjects.Add(new SceneObject(new Vector3(5, 0, 0), new Vector3(0.5f, 2, 10)));
            _sceneObjects.Add(new SceneObject(new Vector3(0, 0, 5), new Vector3(10, 2, 0.5f)));
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            _deltaTime = (float)args.Time;

            if (!IsFocused)
                return;

            HandleMovement();
        }

        private void HandleMovement()
        {
            Vector3 inputDir = Vector3.Zero;

            // Movement
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.W))
                inputDir += _camera.Forward;
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.S))
                inputDir -= _camera.Forward;
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.A))
                inputDir -= _camera.Right;
            if (KeyboardState.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.D))
                inputDir += _camera.Right;

            inputDir.Y = 0;

            if (inputDir.LengthSquared > 0)
                inputDir = inputDir.Normalized();

            Vector3 proposed = _playerPosition + inputDir * _moveSpeed * _deltaTime;

            // Collision check
            if (!CollidesWithAny(proposed))
            {
                _playerPosition = proposed;
                _camera.Position = _playerPosition;
            }
        }

        private bool CollidesWithAny(Vector3 pos)
        {
            float radius = 0.4f; // player sphere radius

            foreach (var obj in _sceneObjects)
            {
                if (obj.CollidesWithSphere(pos, radius))
                    return true;
            }

            return false;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // TODO: render meshes here later

            SwapBuffers();
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            base.OnMouseMove(e);

            _camera.AddRotation(e.DeltaX, e.DeltaY);
        }
    }

    public class SceneObject
    {
        public Vector3 Position;
        public Vector3 Size; // Half-size extents

        public SceneObject(Vector3 pos, Vector3 size)
        {
            Position = pos;
            Size = size;
        }

        // Player uses a sphere collider
        public bool CollidesWithSphere(Vector3 center, float radius)
        {
            float cx = Math.Clamp(center.X, Position.X - Size.X, Position.X + Size.X);
            float cy = Math.Clamp(center.Y, Position.Y - Size.Y, Position.Y + Size.Y);
            float cz = Math.Clamp(center.Z, Position.Z - Size.Z, Position.Z + Size.Z);

            float distSq =
                (center.X - cx) * (center.X - cx) +
                (center.Y - cy) * (center.Y - cy) +
                (center.Z - cz) * (center.Z - cz);

            return distSq < radius * radius;
        }
    }
}
