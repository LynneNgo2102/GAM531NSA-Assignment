using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;

namespace WindowEngine
{
    public class Game : GameWindow
    {
        private int vertexBufferHandle;
        private int shaderProgramHandle;
        private int vertexArrayHandle;
        private int uModelLocation;
        private float time;

        public Game()
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.Size = new Vector2i(1280, 768);
            this.CenterWindow(this.Size);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();
            GL.ClearColor(0.5f, 0.7f, 0.8f, 1f);

            float[] vertices = new float[]
            {
                // rectangle made from two triangles
                -0.5f, -0.5f, 0.0f,
                 0.5f, -0.5f, 0.0f,
                 0.5f,  0.5f, 0.0f,

                -0.5f, -0.5f, 0.0f,
                 0.5f,  0.5f, 0.0f,
                -0.5f,  0.5f, 0.0f,
            };

            vertexBufferHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            string vertexShaderCode = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                uniform mat4 uModel;

                void main()
                {
                    gl_Position = uModel * vec4(aPosition, 1.0);
                }
            ";

            string fragmentShaderCode = @"
                #version 330 core
                out vec4 FragColor;

                void main()
                {
                    FragColor = vec4(0.6f, 0.2f, 0.8f, 1.0f);
                }
            ";

            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShaderHandle, vertexShaderCode);
            GL.CompileShader(vertexShaderHandle);
            CheckShaderCompile(vertexShaderHandle, "Vertex Shader");

            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderCode);
            GL.CompileShader(fragmentShaderHandle);
            CheckShaderCompile(fragmentShaderHandle, "Fragment Shader");

            shaderProgramHandle = GL.CreateProgram();
            GL.AttachShader(shaderProgramHandle, vertexShaderHandle);
            GL.AttachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.LinkProgram(shaderProgramHandle);

            GL.DetachShader(shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);

            // Get uniform location
            uModelLocation = GL.GetUniformLocation(shaderProgramHandle, "uModel");
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            time += (float)args.Time;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.UseProgram(shaderProgramHandle);
            GL.BindVertexArray(vertexArrayHandle);

            // ----- Matrix operations -----
            Matrix4 model = Matrix4.Identity;

            // Scale (make smaller)
            model *= Matrix4.CreateScale(0.5f, 0.5f, 1.0f);

            // Rotate around Z axis
            model *= Matrix4.CreateRotationZ(time);

            // Translate (move)
            model *= Matrix4.CreateTranslation(0.5f, 0.0f, 0.0f);

            // Send matrix to shader
            GL.UniformMatrix4(uModelLocation, false, ref model);

            // Draw rectangle
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindVertexArray(0);
            SwapBuffers();
        }

        protected override void OnUnload()
        {
            GL.DeleteBuffer(vertexBufferHandle);
            GL.DeleteVertexArray(vertexArrayHandle);
            GL.DeleteProgram(shaderProgramHandle);
            base.OnUnload();
        }

        private void CheckShaderCompile(int shaderHandle, string shaderName)
        {
            GL.GetShader(shaderHandle, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(shaderHandle);
                Console.WriteLine($"Error compiling {shaderName}: {infoLog}");
            }
        }
    }
}
