using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework; // <-- for KeyboardState
using System;
using System.Runtime.InteropServices;

namespace WindowEngine
{
    public class Game
    {
        private readonly Surface screen;
        private float angle = 0f; // rotation angle

        private int textureId;
        private int vao, vbo, shaderProgram;

        // World parameters (can zoom/pan)
        private float worldMinX = -10f, worldMaxX = 10f;
        private float worldMinY = -2f, worldMaxY = 2f;
        private float centerX = 0f, centerY = 0f;
        private float zoom = 1f;

        private KeyboardState keyboard;

        public Game(int width, int height)
        {
            screen = new Surface(width, height);
        }

        // --- Input handling ---
        public void HandleInput(KeyboardState kbd)
        {
            keyboard = kbd;

            // Zoom
            if (kbd.IsKeyDown(Keys.Z)) zoom *= 1.02f;
            if (kbd.IsKeyDown(Keys.X)) zoom /= 1.02f;

            // Pan
            if (kbd.IsKeyDown(Keys.Left)) centerX -= 0.1f * zoom;
            if (kbd.IsKeyDown(Keys.Right)) centerX += 0.1f * zoom;
            if (kbd.IsKeyDown(Keys.Up)) centerY += 0.1f * zoom;
            if (kbd.IsKeyDown(Keys.Down)) centerY -= 0.1f * zoom;
        }

        // Generic world→screen transform
        private int TX(float x)
        {
            float worldWidth = (worldMaxX - worldMinX) * zoom;
            float nx = (x - (centerX - worldWidth / 2f)) / worldWidth; // normalize 0–1
            return (int)(nx * screen.width);
        }

        private int TY(float y)
        {
            float worldHeight = (worldMaxY - worldMinY) * zoom;
            float ny = (y - (centerY - worldHeight / 2f)) / worldHeight; // normalize 0–1
            return (int)((1f - ny) * screen.height); // invert Y
        }

        public void Init()
        {
            // Generate texture
            textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                screen.width, screen.height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);

            // Fullscreen quad
            float[] quadVertices = {
                -1f, -1f, 0f, 0f, 0f,
                 1f, -1f, 0f, 1f, 0f,
                 1f,  1f, 0f, 1f, 1f,
                -1f,  1f, 0f, 0f, 1f
            };

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, quadVertices.Length * sizeof(float), quadVertices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // Shader
            string vertexShaderSrc = @"
                #version 330 core
                layout(location=0) in vec3 aPos;
                layout(location=1) in vec2 aTex;
                out vec2 vTex;
                void main() { gl_Position = vec4(aPos,1.0); vTex = aTex; }";

            string fragmentShaderSrc = @"
                #version 330 core
                in vec2 vTex;
                out vec4 FragColor;
                uniform sampler2D uTex;
                void main() { FragColor = texture(uTex, vTex); }";

            int vs = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vs, vertexShaderSrc);
            GL.CompileShader(vs);
            CheckShaderError(vs, "Vertex Shader");

            int fs = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fs, fragmentShaderSrc);
            GL.CompileShader(fs);
            CheckShaderError(fs, "Fragment Shader");

            shaderProgram = GL.CreateProgram();
            GL.AttachShader(shaderProgram, vs);
            GL.AttachShader(shaderProgram, fs);
            GL.LinkProgram(shaderProgram);
            CheckProgramError(shaderProgram);

            GL.DeleteShader(vs);
            GL.DeleteShader(fs);
        }

        public void Tick()
        {
            screen.Clear(0x000000);

            // Draw axes & function
            DrawAxes();
            DrawFunction();

            // Still keep spinning square
            DrawSpinningSquare();

            // Upload pixels
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            var handle = GCHandle.Alloc(screen.pixels, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = handle.AddrOfPinnedObject();
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, screen.width, screen.height,
                    PixelFormat.Bgra, PixelType.UnsignedByte, ptr);
            }
            finally { handle.Free(); }

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.UseProgram(shaderProgram);
            GL.BindVertexArray(vao);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            angle += 0.01f; // speed
        }

        private void DrawSpinningSquare()
        {
            float size = 1.0f;
            float[] xs = { -size, size, size, -size };
            float[] ys = { -size, -size, size, size };

            int[] sx = new int[4];
            int[] sy = new int[4];

            for (int i = 0; i < 4; i++)
            {
                float rx = xs[i] * (float)Math.Cos(angle) - ys[i] * (float)Math.Sin(angle);
                float ry = xs[i] * (float)Math.Sin(angle) + ys[i] * (float)Math.Cos(angle);

                sx[i] = TX(rx);
                sy[i] = TY(ry);
            }

            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                screen.Line(sx[i], sy[i], sx[next], sy[next], 0xffffff);
            }
        }

        private void DrawAxes()
        {
            // X-axis
            screen.Line(TX(worldMinX), TY(0), TX(worldMaxX), TY(0), 0x00ff00);
            // Y-axis
            screen.Line(TX(0), TY(worldMinY), TX(0), TY(worldMaxY), 0x00ff00);
        }

        private void DrawFunction()
        {
            float step = 0.05f;
            float prevX = worldMinX, prevY = (float)Math.Sin(prevX);

            for (float x = worldMinX + step; x <= worldMaxX; x += step)
            {
                float y = (float)Math.Sin(x);

                screen.Line(TX(prevX), TY(prevY), TX(x), TY(y), 0xff0000);

                prevX = x;
                prevY = y;
            }
        }

        public void Cleanup()
        {
            GL.DeleteTexture(textureId);
            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
        }

        private void CheckShaderError(int shader, string name)
        {
            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0) Console.WriteLine($"{name} Compilation Error: {GL.GetShaderInfoLog(shader)}");
        }

        private void CheckProgramError(int program)
        {
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0) Console.WriteLine($"Program Link Error: {GL.GetProgramInfoLog(program)}");
        }
    }

    public class Surface
    {
        public int[] pixels;
        public int width, height;

        public Surface(int w, int h)
        {
            width = w;
            height = h;
            pixels = new int[w * h];
        }

        public void Clear(int color)
        {
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
        }

        public void Line(int x0, int y0, int x1, int y1, int color)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy, e2;
            while (true)
            {
                if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
                    pixels[y0 * width + x0] = color;
                if (x0 == x1 && y0 == y1) break;
                e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }
    }
}
