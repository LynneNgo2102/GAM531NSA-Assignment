using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace BreakoutModernOpenTK
{
    class GameObject
    {
        public Vector2 Position;
        public Vector2 Size;
        public bool Destroyed = false;
        public Vector3 Color = new Vector3(1f);
        public GameObject(Vector2 pos, Vector2 size) { Position = pos; Size = size; }
        public Vector2 Center => Position + Size * 0.5f;
    }

    class BallObject
    {
        public Vector2 Center;
        public float Radius;
        public Vector2 Velocity;
        public Vector3 Color = new Vector3(1f, 0.7f, 0.2f);
        public bool Stuck = true;
        public BallObject(Vector2 c, float r) { Center = c; Radius = r; Velocity = new Vector2(0, -200); }
    }

    class SimpleShader : IDisposable
    {
        public int Handle;
        public SimpleShader(string vsSrc, string fsSrc)
        {
            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vsSrc);
            GL.CompileShader(vs);
            CheckShader(vs);

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fsSrc);
            GL.CompileShader(fs);
            CheckShader(fs);

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, vs);
            GL.AttachShader(Handle, fs);
            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int ok);
            if (ok == 0) throw new Exception("Program link error: " + GL.GetProgramInfoLog(Handle));

            GL.DetachShader(Handle, vs);
            GL.DetachShader(Handle, fs);
            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
        }

        void CheckShader(int id)
        {
            GL.GetShader(id, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0) throw new Exception("Shader compile error: " + GL.GetShaderInfoLog(id));
        }

        public void Use() => GL.UseProgram(Handle);
        public int GetUniformLocation(string name) => GL.GetUniformLocation(Handle, name);
        public void Dispose() { if (Handle != 0) GL.DeleteProgram(Handle); }
    }
   
    class BreakoutWindow : GameWindow
    {
        int ScreenW = 800, ScreenH = 600;
        GameObject paddle;
        BallObject ball;
        List<GameObject> bricks = new List<GameObject>();
        float paddleSpeed = 500f;

        // shader + uniform locations
        SimpleShader shader;
        int uProjection, uModel, uColor;

        // buffers for dynamic draws
        int vao, vbo;
       
        public BreakoutWindow(GameWindowSettings gws, NativeWindowSettings nws) : base(gws, nws) { }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.06f, 0.06f, 0.08f, 1f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // simple shader
            string vs = @"
                #version 330 core
                layout(location = 0) in vec2 aPos;
                uniform mat4 uProjection;
                uniform mat4 uModel;
                void main() {
                    gl_Position = uProjection * uModel * vec4(aPos, 0.0, 1.0);
                }";
            string fs = @"
                #version 330 core
                uniform vec3 uColor;
                out vec4 FragColor;
                void main() {
                    FragColor = vec4(uColor, 1.0);
                }";
            shader = new SimpleShader(vs, fs);
            shader.Use();
            uProjection = shader.GetUniformLocation("uProjection");
            uModel = shader.GetUniformLocation("uModel");
            uColor = shader.GetUniformLocation("uColor");

            // Create VAO/VBO (we will upload vertex arrays per-draw)
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            ResetLevel();
            UpdateProjection();
        }

        void UpdateProjection()
        {
            Matrix4 proj = Matrix4.CreateOrthographicOffCenter(0, ScreenW, ScreenH, 0, -1f, 1f);
            shader.Use();
            GL.UniformMatrix4(uProjection, false, ref proj);
        }

        void ResetLevel()
        {
            bricks.Clear();
            Vector2 paddleSize = new Vector2(120, 20);
            paddle = new GameObject(new Vector2((ScreenW - paddleSize.X) / 2f, ScreenH - 50), paddleSize);
            paddle.Color = new Vector3(0.2f, 0.6f, 1.0f);

            ball = new BallObject(new Vector2(paddle.Center.X, paddle.Position.Y - 12f), 10f);
            ball.Stuck = true;
            ball.Velocity = new Vector2(150f, -250f);

            // bricks
            int rows = 5, cols = 10;
            float bw = 64, bh = 22;
            float offX = 40, offY = 30, spacingX = 6, spacingY = 8;
            for (int r = 0; r < rows; ++r)
            {
                for (int c = 0; c < cols; ++c)
                {
                    float x = offX + c * (bw + spacingX);
                    float y = offY + r * (bh + spacingY);
                    var b = new GameObject(new Vector2(x, y), new Vector2(bw, bh));
                    b.Color = new Vector3(0.25f + 0.1f * r, 0.45f, 0.7f - 0.08f * r);
                    bricks.Add(b);
                }
            }
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            ScreenW = Size.X; ScreenH = Size.Y;
            GL.Viewport(0, 0, ScreenW, ScreenH);
            UpdateProjection();
        }

        // --- drawing primitives using shader + VAO/VBO ---
        void DrawRect(float x, float y, float w, float h, Vector3 color)
        {
            // two triangles (6 vertices)
            float[] verts = {
                x,     y,
                x + w, y,
                x + w, y + h,
                x,     y,
                x + w, y + h,
                x,     y + h
            };

            shader.Use();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.DynamicDraw);

            // model is identity for rects since we compute coords directly in screen space
            Matrix4 model = Matrix4.Identity;
            GL.UniformMatrix4(uModel, false, ref model);
            GL.Uniform3(uColor, color);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        void DrawCircle(float cx, float cy, float radius, Vector3 color, int segments = 40)
        {
            // triangle fan: center + perimeter points
            float[] verts = new float[(segments + 2) * 2];
            verts[0] = cx; verts[1] = cy;
            for (int i = 0; i <= segments; i++)
            {
                float theta = i * (float)(Math.PI * 2) / segments;
                verts[(i + 1) * 2 + 0] = cx + radius * MathF.Cos(theta);
                verts[(i + 1) * 2 + 1] = cy + radius * MathF.Sin(theta);
            }

            shader.Use();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.DynamicDraw);

            Matrix4 model = Matrix4.Identity;
            GL.UniformMatrix4(uModel, false, ref model);
            GL.Uniform3(uColor, color);

            GL.DrawArrays(PrimitiveType.TriangleFan, 0, segments + 2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        // clamp helper
        static float Clamp(float v, float a, float b) => MathF.Max(a, MathF.Min(b, v));

        static bool CircleAABBOverlap(BallObject ball, GameObject rect, out Vector2 closest)
        {
            float rectMinX = rect.Position.X;
            float rectMaxX = rect.Position.X + rect.Size.X;
            float rectMinY = rect.Position.Y;
            float rectMaxY = rect.Position.Y + rect.Size.Y;

            float cx = Clamp(ball.Center.X, rectMinX, rectMaxX);
            float cy = Clamp(ball.Center.Y, rectMinY, rectMaxY);
            closest = new Vector2(cx, cy);
            float dx = ball.Center.X - cx;
            float dy = ball.Center.Y - cy;
            return (dx * dx + dy * dy) <= ball.Radius * ball.Radius;
        }

        static bool AABBOverlap(GameObject a, GameObject b)
        {
            bool ox = a.Position.X < b.Position.X + b.Size.X && a.Position.X + a.Size.X > b.Position.X;
            bool oy = a.Position.Y < b.Position.Y + b.Size.Y && a.Position.Y + a.Size.Y > b.Position.Y;
            return ox && oy;
        }

        void ResolveCircleAABB(BallObject ball, GameObject rect)
        {
            // Find which axis to reflect on by comparing overlap distances
            Vector2 rectCenter = rect.Center;
            Vector2 diff = ball.Center - rectCenter;
            Vector2 half = rect.Size * 0.5f;
            float dx = MathF.Abs(diff.X) - half.X;
            float dy = MathF.Abs(diff.Y) - half.Y;

            if (MathF.Abs(dx) < MathF.Abs(dy))
                ball.Velocity.X = -ball.Velocity.X;
            else
                ball.Velocity.Y = -ball.Velocity.Y;
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            float dt = (float)e.Time;
            var k = KeyboardState;

            if (k.IsKeyDown(Keys.Escape)) Close();

            float dir = 0;
            if (k.IsKeyDown(Keys.Left) || k.IsKeyDown(Keys.A)) dir -= 1;
            if (k.IsKeyDown(Keys.Right) || k.IsKeyDown(Keys.D)) dir += 1;
            paddle.Position.X += dir * paddleSpeed * dt;
            paddle.Position.X = Clamp(paddle.Position.X, 0, ScreenW - paddle.Size.X);

            if (k.IsKeyPressed(Keys.R)) ResetLevel();
            if (k.IsKeyPressed(Keys.Space) && ball.Stuck) { ball.Stuck = false; var rnd = new Random(); ball.Velocity = new Vector2((float)(rnd.NextDouble() * 200 - 100), -200f); }

            if (ball.Stuck)
            {
                ball.Center = new Vector2(paddle.Center.X, paddle.Position.Y - ball.Radius - 1f);
            }
            else
            {
                // integrate
                ball.Center += ball.Velocity * dt;

                // bounds
                if (ball.Center.X - ball.Radius < 0) { ball.Center.X = ball.Radius; ball.Velocity.X = -ball.Velocity.X; }
                if (ball.Center.X + ball.Radius > ScreenW) { ball.Center.X = ScreenW - ball.Radius; ball.Velocity.X = -ball.Velocity.X; }
                if (ball.Center.Y - ball.Radius < 0) { ball.Center.Y = ball.Radius; ball.Velocity.Y = -ball.Velocity.Y; }

                if (ball.Center.Y - ball.Radius > ScreenH) { ball.Stuck = true; ball.Velocity = new Vector2(0, -200); ball.Center = new Vector2(paddle.Center.X, paddle.Position.Y - ball.Radius - 1f); }

                // paddle
                if (CircleAABBOverlap(ball, paddle, out Vector2 cp))
                {
                    float offset = (ball.Center.X - paddle.Center.X) / (paddle.Size.X / 2f);
                    float strength = 300f;
                    ball.Velocity.X = strength * offset;
                    ball.Velocity.Y = -MathF.Abs(ball.Velocity.Y);
                    ball.Center.Y = paddle.Position.Y - ball.Radius - 1f;
                }

                // bricks
                foreach (var b in bricks.Where(b => !b.Destroyed))
                {
                    if (CircleAABBOverlap(ball, b, out Vector2 cp2))
                    {
                        b.Destroyed = true;
                        ResolveCircleAABB(ball, b);
                        ball.Velocity *= 1.02f;
                        break;
                    }
                }
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // draw bricks
            foreach (var b in bricks) if (!b.Destroyed) DrawRect(b.Position.X, b.Position.Y, b.Size.X, b.Size.Y, b.Color);

            // paddle
            DrawRect(paddle.Position.X, paddle.Position.Y, paddle.Size.X, paddle.Size.Y, paddle.Color);

            // ball
            DrawCircle(ball.Center.X, ball.Center.Y, ball.Radius, ball.Color, 40);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            shader.Dispose();
        }
    }

    static class Program
    {
        static void Main()
        {
            var gws = GameWindowSettings.Default;
            var nws = new NativeWindowSettings()
            {
                Size = new Vector2i(800, 600),
                Title = "Breakout (modern OpenTK) - AABB & Circle collisions"
            };
            using var win = new BreakoutWindow(gws, nws);
            win.Run();
        }
    }
}
