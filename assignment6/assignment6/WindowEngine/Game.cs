using System;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;

using StbImageSharp;

namespace WindowEngine
{
    public class Game : GameWindow
    {
        private int vbo, vao, ebo, shaderProgram;
        private int textureId;

        private float[] cubeVertices =
 {
    // positions        // texcoords
    // Front face
    -0.5f, -0.5f,  0.5f,  0f, 0f,
     0.5f, -0.5f,  0.5f,  1f, 0f,
     0.5f,  0.5f,  0.5f,  1f, 1f,
    -0.5f,  0.5f,  0.5f,  0f, 1f,

    // Back face
    -0.5f, -0.5f, -0.5f,  1f, 0f,
     0.5f, -0.5f, -0.5f,  0f, 0f,
     0.5f,  0.5f, -0.5f,  0f, 1f,
    -0.5f,  0.5f, -0.5f,  1f, 1f,

    // Left face
    -0.5f, -0.5f, -0.5f,  0f, 0f,
    -0.5f, -0.5f,  0.5f,  1f, 0f,
    -0.5f,  0.5f,  0.5f,  1f, 1f,
    -0.5f,  0.5f, -0.5f,  0f, 1f,

    // Right face
     0.5f, -0.5f, -0.5f,  1f, 0f,
     0.5f, -0.5f,  0.5f,  0f, 0f,
     0.5f,  0.5f,  0.5f,  0f, 1f,
     0.5f,  0.5f, -0.5f,  1f, 1f,

    // Top face
    -0.5f,  0.5f,  0.5f,  0f, 0f,
     0.5f,  0.5f,  0.5f,  1f, 0f,
     0.5f,  0.5f, -0.5f,  1f, 1f,
    -0.5f,  0.5f, -0.5f,  0f, 1f,

    // Bottom face
    -0.5f, -0.5f,  0.5f,  0f, 1f,
     0.5f, -0.5f,  0.5f,  1f, 1f,
     0.5f, -0.5f, -0.5f,  1f, 0f,
    -0.5f, -0.5f, -0.5f,  0f, 0f
};

        private uint[] indices =
        {
    0, 1, 2, 2, 3, 0,       // Front
    4, 5, 6, 6, 7, 4,       // Back
    8, 9, 10, 10, 11, 8,    // Left
    12, 13, 14, 14, 15, 12, // Right
    16, 17, 18, 18, 19, 16, // Top
    20, 21, 22, 22, 23, 20  // Bottom
}; 
        
        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _vertexArrayObject;
        private Shader _shader;
        
        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        private double _time;


        public Game(GameWindowSettings gameSettings, NativeWindowSettings nativeSettings)
            : base(gameSettings, nativeSettings)
        {
            CenterWindow(nativeSettings.ClientSize);
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            string vertexShaderCode = @"
                #version 330 core

                layout(location = 0) in vec3 aPosition;

                layout(location = 1) in vec2 aTexCoord;

                out vec2 texCoord;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                void main(void)
                 {
                   texCoord = aTexCoord;

                   gl_Position = projection * view * model * vec4(aPosition, 1.0);
                  }
            ";

            string fragmentShaderCode = @"
                #version 330 core
                in vec2 texCoord;
                out vec4 outputColor;

                uniform sampler2D uTexture;
                uniform sampler2D uTexture2;

                void main()
                {
                   outputColor = mix(texture(uTexture, texCoord), texture(uTexture2, texCoord), 0.2);

                }
            ";
            shaderProgram = CreateShaderProgram(vertexShaderCode, fragmentShaderCode);

            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();
            vao = GL.GenVertexArray();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, cubeVertices.Length * sizeof(float), cubeVertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

            var vertexLocation = GL.GetAttribLocation(shaderProgram, "aPosition");
            GL.EnableVertexAttribArray(vertexLocation);


            
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);

            var textCoordLocation = GL.GetAttribLocation(shaderProgram, "aTexCoord");
            GL.EnableVertexAttribArray(textCoordLocation);

            GL.VertexAttribPointer(textCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));
           // GL.EnableVertexAttribArray(1);

            

            

            textureId = LoadTexture("wall.jpg");

            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);
            CursorState = CursorState.Grabbed;
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            //  Update time-based rotation (smooth animation)
            _time += 1.0 * args.Time;

            //  Clear the screen and depth buffer
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //  Use our VAO
            GL.BindVertexArray(vao);

            //  Bind the texture(s)
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            //  Activate shader program
            GL.UseProgram(shaderProgram);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "uTexture"), 0);
            //  Create transform matrices
            //Matrix4 model = Matrix4.CreateRotationY((float)MathHelper.DegreesToRadians(_time));
            Matrix4 model = Matrix4.CreateRotationY((float)_time);
            Matrix4 view = _camera.GetViewMatrix();
            Matrix4 projection = _camera.GetProjectionMatrix();

            //  Send matrices to the shader
            int modelLoc = GL.GetUniformLocation(shaderProgram, "model");
            int viewLoc = GL.GetUniformLocation(shaderProgram, "view");
            int projLoc = GL.GetUniformLocation(shaderProgram, "projection");

            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projLoc, false, ref projection);

            //  Draw the cube
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            //  Swap front/back buffers
            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused) // Check to see if the window is focused
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                _camera.Position += _camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            // Get the mouse state
            var mouse = MouseState;

            if (_firstMove) // This bool variable is initially set to true.
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Rotate camera only when left mouse button is held(if you want to enable this, uncomment the if statement)
                //if (mouse.IsButtonDown(MouseButton.Left))
                //{
                _camera.Yaw += deltaX * sensitivity;
                    _camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
               // }
            }
        }

        // In the mouse wheel function, we manage all the zooming of the camera.
        // This is simply done by changing the FOV of the camera.
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _camera.Fov -= e.OffsetY;
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            // We need to update the aspect ratio once the window has been resized.
            if (_camera != null)
            {
                _camera.AspectRatio = Size.X / (float)Size.Y;
            }
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

            //Texture wrapping setup
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);

            // Filtering (smooth scaling)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            float[] borderColor = { 1f, 1f, 0f, 1f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            return tex;
        }



    }
}
