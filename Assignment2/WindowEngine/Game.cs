using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using System.Drawing;
using System.Drawing.Imaging;


namespace WindowEngine
{
    public class Game : GameWindow
    {
        // GPU handles
        private int vbo, vao, ebo, shaderProgram;
        private int textureId;

        public Game()
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            Size = new Vector2i(1280, 768);
            CenterWindow(Size);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(new Color4(0.5f, 0.7f, 0.8f, 1f));
            GL.Enable(EnableCap.DepthTest);

            // Quad vertices: position (3) + texcoords (2)
            float[] vertices =
            {
                // X     Y     Z     U   V
                -0.5f, -0.5f, 0f,   0f, 0f,
                 0.5f, -0.5f, 0f,   1f, 0f,
                 0.5f,  0.5f, 0f,   1f, 1f,
                -0.5f,  0.5f, 0f,   0f, 1f
            };

            uint[] indices = { 0, 1, 2, 2, 3, 0 };

            // VBO
            vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // EBO
            ebo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            // VAO
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            // Shaders (no matrices)
            string vertexShaderCode = @"
                #version 330 core
                layout (location = 0) in vec3 aPosition;
                layout (location = 1) in vec2 aTexCoord;

                out vec2 TexCoord;

                void main()
                {
                    gl_Position = vec4(aPosition, 1.0);
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

            // Load texture (make sure you have texture.png in project folder)
            textureId = LoadTexture("texture.png");
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.UseProgram(shaderProgram);

            // Bind texture
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            // Draw
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
            GL.DeleteTexture(textureId);
            base.OnUnload();
        }

        // Helpers
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

            GL.DetachShader(program, vShader);
            GL.DetachShader(program, fShader);
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

            using (Bitmap bmp = new Bitmap(path))
            {
                var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                        ImageLockMode.ReadOnly,
                                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                              bmp.Width, bmp.Height, 0,
                              OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bmp.UnlockBits(data);
            }

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            return tex;
        }

        
    }
}
