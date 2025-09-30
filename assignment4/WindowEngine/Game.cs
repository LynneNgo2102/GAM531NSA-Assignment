using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using StbImageSharp;

namespace WindowEngine
{
    public class Game : GameWindow
    {
        private int vbo, vao, ebo, shaderProgram;
        private int textureId;

        private float[] cubeVertices =
        {
            // positions         // texcoords
            // Front
            -0.5f, -0.5f,  0.5f,  0f, 0f,
             0.5f, -0.5f,  0.5f,  1f, 0f,
             0.5f,  0.5f,  0.5f,  1f, 1f,
            -0.5f,  0.5f,  0.5f,  0f, 1f,

            // Back
            -0.5f, -0.5f, -0.5f,  1f, 0f,
             0.5f, -0.5f, -0.5f,  0f, 0f,
             0.5f,  0.5f, -0.5f,  0f, 1f,
            -0.5f,  0.5f, -0.5f,  1f, 1f,
        };

        private uint[] indices =
        {
            // Front
            0, 1, 2, 2, 3, 0,
            // Back
            4, 5, 6, 6, 7, 4,
            // Left
            4, 0, 3, 3, 7, 4,
            // Right
            1, 5, 6, 6, 2, 1,
            // Top
            3, 2, 6, 6, 7, 3,
            // Bottom
            4, 5, 1, 1, 0, 4
        };

        public Game()
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            Size = new Vector2i(800, 600);
            CenterWindow(Size);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(Color4.CornflowerBlue);
            GL.Enable(EnableCap.DepthTest);

            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();
            vao = GL.GenVertexArray();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, cubeVertices.Length * sizeof(float), cubeVertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            string vertexShaderCode = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in vec2 aTexCoord;

                uniform mat4 uMVP;

                out vec2 TexCoord;

                void main()
                {
                    gl_Position = uMVP * vec4(aPosition, 1.0);
                    TexCoord = aTexCoord;
                }
            ";

            string fragmentShaderCode = @"
                #version 330 core
                in vec2 TexCoord;
                out vec4 FragColor;

                uniform sampler2D uTexture;

                void main()
                {
                    FragColor = texture(uTexture, TexCoord);
                }
            ";

            shaderProgram = CreateShaderProgram(vertexShaderCode, fragmentShaderCode);

            textureId = LoadTexture("brick-texture.jpg");
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(shaderProgram);

            Matrix4 model = Matrix4.CreateRotationY((float)DateTime.Now.TimeOfDay.TotalSeconds);
            Matrix4 view = Matrix4.LookAt(new Vector3(1.5f, 1.5f, 3f), Vector3.Zero, Vector3.UnitY);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f),
                Size.X / (float)Size.Y, 0.1f, 100f);

            Matrix4 mvp = model * view * projection;
            int mvpLoc = GL.GetUniformLocation(shaderProgram, "uMVP");
            GL.UniformMatrix4(mvpLoc, false, ref mvp);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        private int CreateShaderProgram(string vertexCode, string fragmentCode)
        {
            int vShader = CompileShader(ShaderType.VertexShader, vertexCode);
            int fShader = CompileShader(ShaderType.FragmentShader, fragmentCode);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vShader);
            GL.AttachShader(program, fShader);
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
                Console.WriteLine(GL.GetProgramInfoLog(program));

            GL.DeleteShader(vShader);
            GL.DeleteShader(fShader);
            return program;
        }

        private int CompileShader(ShaderType type, string code)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
                Console.WriteLine(GL.GetShaderInfoLog(shader));

            return shader;
        }

        private int LoadTexture(string path)
        {
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            using (var stream = File.OpenRead(path))
            {
                var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                    image.Width, image.Height, 0,
                    OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
            }

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            return tex;
        }
    }
}
