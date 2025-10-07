using System;
using System.IO;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
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
    // positions          // normals
    // Front face
    -0.5f, -0.5f,  0.5f,   0f,  0f,  1f,
     0.5f, -0.5f,  0.5f,   0f,  0f,  1f,
     0.5f,  0.5f,  0.5f,   0f,  0f,  1f,
    -0.5f,  0.5f,  0.5f,   0f,  0f,  1f,

    // Back face
    -0.5f, -0.5f, -0.5f,   0f,  0f, -1f,
     0.5f, -0.5f, -0.5f,   0f,  0f, -1f,
     0.5f,  0.5f, -0.5f,   0f,  0f, -1f,
    -0.5f,  0.5f, -0.5f,   0f,  0f, -1f,

    // Left face
    -0.5f, -0.5f, -0.5f,  -1f,  0f,  0f,
    -0.5f, -0.5f,  0.5f,  -1f,  0f,  0f,
    -0.5f,  0.5f,  0.5f,  -1f,  0f,  0f,
    -0.5f,  0.5f, -0.5f,  -1f,  0f,  0f,

    // Right face
     0.5f, -0.5f, -0.5f,   1f,  0f,  0f,
     0.5f, -0.5f,  0.5f,   1f,  0f,  0f,
     0.5f,  0.5f,  0.5f,   1f,  0f,  0f,
     0.5f,  0.5f, -0.5f,   1f,  0f,  0f,

    // Top face
    -0.5f,  0.5f,  0.5f,   0f,  1f,  0f,
     0.5f,  0.5f,  0.5f,   0f,  1f,  0f,
     0.5f,  0.5f, -0.5f,   0f,  1f,  0f,
    -0.5f,  0.5f, -0.5f,   0f,  1f,  0f,

    // Bottom face
    -0.5f, -0.5f,  0.5f,   0f, -1f,  0f,
     0.5f, -0.5f,  0.5f,   0f, -1f,  0f,
     0.5f, -0.5f, -0.5f,   0f, -1f,  0f,
    -0.5f, -0.5f, -0.5f,   0f, -1f,  0f
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
        //orbit camera parameters
        private float cameraDistance = 3.0f;
        private float cameraYaw = 45f;
        private float cameraPitch = 20f;

        //model rotation
        private float modelYaw = 0f;
        private float modelPitch = 0f;

        //Light properties
        private Vector3 lightPos = new Vector3(2f, 2f, 2f);
        private Vector3 lightColor = new Vector3(1.0f, 1.0f, 1.0f);
        private Vector3 objectColor = new Vector3(0.8f, 0.5f, 0.3f);
        public Game()
            : base(GameWindowSettings.Default, NativeWindowSettings.Default)
        {
            Size = new Vector2i(800, 600);
            CenterWindow(Size);
            //CursorGrabbed = false;
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(Color4.CornflowerBlue);
            GL.Enable(EnableCap.DepthTest);

            //Create buffers/vao
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();
            vao = GL.GenVertexArray();

            GL.BindVertexArray(vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, cubeVertices.Length * sizeof(float), cubeVertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);


            // Position attribute
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // Normal attribute
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            // ----- Shaders (Phong) -----
            string vertexShaderCode = @"
                #version 330 core
                layout(location = 0) in vec3 aPosition;
                layout(location = 1) in vec3 aNormal;

                out vec3 FragPos;
                out vec3 Normal;

                uniform mat4 model;
                uniform mat4 view;
                uniform mat4 projection;

                void main()
                {
                    // world space position of fragment
                    FragPos = vec3(model * vec4(aPosition, 1.0));
                    // transform normal correctly
                    Normal = mat3(transpose(inverse(model))) * aNormal;
                    gl_Position = projection * view * vec4(FragPos, 1.0);
                }
            ";

            string fragmentShaderCode = @"
                #version 330 core
                out vec4 FragColor;

                in vec3 FragPos;
                in vec3 Normal;

                uniform vec3 lightPos;
                uniform vec3 viewPos;
                uniform vec3 lightColor;
                uniform vec3 objectColor;

                void main()
                {
                    // Ambient
                    float ambientStrength = 0.1;
                    vec3 ambient = ambientStrength * lightColor;

                    // Diffuse
                    vec3 norm = normalize(Normal);
                    vec3 lightDir = normalize(lightPos - FragPos);
                    float diff = max(dot(norm, lightDir), 0.0);
                    vec3 diffuse = diff * lightColor;

                    // Specular (Blinn-Phong variant would use halfway vector; using classic reflect)
                    float specularStrength = 0.6;
                    vec3 viewDir = normalize(viewPos - FragPos);
                    vec3 reflectDir = reflect(-lightDir, norm);
                    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
                    vec3 specular = specularStrength * spec * lightColor;

                    vec3 result = (ambient + diffuse + specular) * objectColor;
                    FragColor = vec4(result, 1.0);
                }
            ";

            shaderProgram = CreateShaderProgram(vertexShaderCode, fragmentShaderCode);

            // set initial uniform values that don't change often
            GL.UseProgram(shaderProgram);
            int lightColorLoc = GL.GetUniformLocation(shaderProgram, "lightColor");
            int objectColorLoc = GL.GetUniformLocation(shaderProgram, "objectColor");
            GL.Uniform3(lightColorLoc, lightColor);
            GL.Uniform3(objectColorLoc, objectColor);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            var kb = KeyboardState;

            //Camera controls: arrow keys to rotate around the model
            if (kb.IsKeyDown(Keys.Left)) cameraYaw -= 60f * (float)args.Time;
            if (kb.IsKeyDown(Keys.Right)) cameraYaw += 60f * (float)args.Time;
            if (kb.IsKeyDown(Keys.Up)) cameraPitch = Math.Clamp(cameraPitch + 60f * (float)args.Time, -89f, 89f);
            if (kb.IsKeyDown(Keys.Down)) cameraPitch = MathHelper.Clamp(cameraPitch - 60f * (float)args.Time, -89f, 89f);
            if (kb.IsKeyDown(Keys.Z)) cameraDistance = MathF.Max(1f, cameraDistance - 2f * (float)args.Time);
            if (kb.IsKeyDown(Keys.X)) cameraDistance = MathF.Min(10f, cameraDistance + 2f * (float)args.Time);

            // Model rotation controls (Q/E)
            if (kb.IsKeyDown(Keys.Q)) modelYaw -= 60f * (float)args.Time;
            if (kb.IsKeyDown(Keys.E)) modelYaw += 60f * (float)args.Time;

            // Light movement (WASD + R/F)
            if (kb.IsKeyDown(Keys.W)) lightPos.Z -= 2f * (float)args.Time;
            if (kb.IsKeyDown(Keys.S)) lightPos.Z += 2f * (float)args.Time;
            if (kb.IsKeyDown(Keys.A)) lightPos.X -= 2f * (float)args.Time;
            if (kb.IsKeyDown(Keys.D)) lightPos.X += 2f * (float)args.Time;
            if (kb.IsKeyDown(Keys.R)) lightPos.Y += 2f * (float)args.Time;
            if (kb.IsKeyDown(Keys.F)) lightPos.Y -= 2f * (float)args.Time;

            // Escape to close
            if (kb.IsKeyDown(Keys.Escape)) Close();
        }


        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(shaderProgram);

            Matrix4 model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(modelYaw)) *
                            Matrix4.CreateRotationX(MathHelper.DegreesToRadians(modelPitch));

            // Camera position from spherical coordinates (yaw/pitch in degrees)
            float yawRad = MathHelper.DegreesToRadians(cameraYaw);
            float pitchRad = MathHelper.DegreesToRadians(cameraPitch);
            Vector3 cameraPos = new Vector3(
                cameraDistance * MathF.Cos(pitchRad) * MathF.Cos(yawRad),
                cameraDistance * MathF.Sin(pitchRad),
                cameraDistance * MathF.Cos(pitchRad) * MathF.Sin(yawRad)
            );


            Matrix4 view = Matrix4.LookAt(cameraPos, Vector3.Zero, Vector3.UnitY);

            float aspect = Size.X / (float)Size.Y;
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), aspect, 0.1f, 100f);

            // Pass uniforms

            int modelLoc = GL.GetUniformLocation(shaderProgram, "model");
            int viewLoc = GL.GetUniformLocation(shaderProgram, "view");
            int projLoc = GL.GetUniformLocation(shaderProgram, "projection");
            int lightPosLoc = GL.GetUniformLocation(shaderProgram, "lightPos");
            int viewPosLoc = GL.GetUniformLocation(shaderProgram, "viewPos");

            GL.UniformMatrix4(modelLoc, false, ref model);
            GL.UniformMatrix4(viewLoc, false, ref view);
            GL.UniformMatrix4(projLoc, false, ref projection);
            GL.Uniform3(lightPosLoc, lightPos);
            GL.Uniform3(viewPosLoc, cameraPos);

            // bind and draw
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);

            SwapBuffers();
        }

        protected override void OnUnload()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
            base.OnUnload();
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

        //private int LoadTexture(string path)
        //{
        //    int tex = GL.GenTexture();
        //    GL.BindTexture(TextureTarget.Texture2D, tex);

        //    using (var stream = File.OpenRead(path))
        //    {
        //        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        //        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 
        //            image.Width, image.Height, 0,
        //            OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
        //    }

        //    //Texture wrapping setup
        //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.MirroredRepeat);
        //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.MirroredRepeat);

        //    // Filtering (smooth scaling)
        //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        //    float[] borderColor = { 1f, 1f, 0f, 1f };
        //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

        //    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        //    return tex;
        //}

    }
}
