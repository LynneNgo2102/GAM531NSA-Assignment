using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace WindowEngine
{
    public class Game : GameWindow
    {
        private int vertexBufferHandle;
        private int shaderProgramHandle;
        private int vertexArrayHandle;
        private int eboHandle;
        private uint[] indices;
        private float rotationAngle = 0f;

        public Game() : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            this.Size = new Vector2i(768, 768);

            this.CenterWindow(this.Size);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(new Color4(0.9098f, 0.9098f, 0.9098f, 1f));
            GL.Enable(EnableCap.DepthTest);

            

            // using framebuffer size here due to potential discrepancy between
            // window size and framebuffer size on MacOS
            GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);

            float[] vertices = {
                // Front face (red)
                -0.5f, -0.5f,  0.5f,   1f, 0f, 0f,
                 0.5f, -0.5f,  0.5f,   1f, 0f, 0f,
                 0.5f,  0.5f,  0.5f,   0f, 1f, 0f,
                -0.5f,  0.5f,  0.5f,   1f, 1f, 0f,

                // Back face (green)
                -0.5f, -0.5f, -0.5f,   0f, 0f, 1f,
                 0.5f, -0.5f, -0.5f,   0f, 0f, 1f,
                 0.5f,  0.5f, -0.5f,   1f, 1f, 0f,
                -0.5f,  0.5f, -0.5f,   1f, 1f, 0f,


            };
            // Indices for 12 triangles (6 faces)
            indices = new uint[]
            {
                0, 1, 2, 2, 3, 0,
                4, 5, 6, 6, 7, 4,
                4, 0, 3, 3, 7, 4,
                1, 5, 6, 6, 2, 1,
                3, 2, 6, 6, 7, 3,
                4, 5, 1, 1, 0, 4
            };

            vertexBufferHandle = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            vertexArrayHandle = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayHandle);

            eboHandle = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboHandle);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferHandle);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            string vertexShaderCode = @"
                #version 330 core
                
                layout(location = 0) in vec3 aPosition;
                layout(location = 1) in vec3 aColor; // Input color attribute

                uniform mat4 uMVP;

                out vec3 vColor;

                void main() 
                {
                    gl_Position = uMVP * vec4(aPosition, 1.0);
                    vColor = aColor;
                }
            ";

            string fragmentShaderCode = @"
                #version 330 core
                
                in vec3 vColor;
                out vec4 FragColor;
                
                void main()
                {
                    FragColor =  vec4(vColor, 1.0);
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

            GL.GetProgram(shaderProgramHandle, GetProgramParameterName.LinkStatus, out int linkStatus);
            if (linkStatus == 0)
            {
                string infoLog = GL.GetProgramInfoLog(shaderProgramHandle);
                Console.WriteLine($"Error linking shader program: {infoLog}");
            }

            GL.DetachShader(shaderProgramHandle, vertexShaderHandle);
            GL.DetachShader(shaderProgramHandle, fragmentShaderHandle);
            GL.DeleteShader(vertexShaderHandle);
            GL.DeleteShader(fragmentShaderHandle);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(shaderProgramHandle);
            //Disable this line to interaction
            rotationAngle += 0.0002f; // Adjust speed as needed
            Matrix4 model = Matrix4.CreateRotationY(rotationAngle);

           
            Matrix4 view = Matrix4.LookAt(new Vector3(1.5f, 1.5f, 1.5f), Vector3.Zero, Vector3.UnitY);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), Size.X / (float)Size.Y, 0.1f, 100f);
            Matrix4 mvp = model * view * projection;

            int mvpLocation = GL.GetUniformLocation(shaderProgramHandle, "uMVP");
            GL.UniformMatrix4(mvpLocation, false, ref mvp);

            GL.BindVertexArray(vertexArrayHandle);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0); // reset
            

            // DrawArrays drew to the back buffer, so now we have to swap the
            // buffers to display on the screen buffer
            SwapBuffers();
        }

        protected override void OnUnload()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DeleteBuffer(vertexBufferHandle);

            GL.BindVertexArray(0);
            GL.DeleteVertexArray(vertexArrayHandle);

            GL.UseProgram(0);
            GL.DeleteProgram(shaderProgramHandle);

            base.OnUnload();
        }
        //using left/right arrow key to rotation, if you want to use this please disable the rotationAngle speed
        //in OnRenderFrame()
        //protected override void OnUpdateFrame(FrameEventArgs args)
        //{
        //    base.OnUpdateFrame(args);

        //    var input = KeyboardState;

        //    if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Left))
        //    {
        //        rotationAngle -= 0.05f;
        //    }
        //    if (input.IsKeyDown(OpenTK.Windowing.GraphicsLibraryFramework.Keys.Right))
        //    {
        //        rotationAngle += 0.05f;
        //    }
        //}
        // Helper function to check for shader compilation errors
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