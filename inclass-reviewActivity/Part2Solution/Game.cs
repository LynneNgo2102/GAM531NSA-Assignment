using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace WindowEngine
{
    public class Game
    {
        private int width, height;
        private int shaderProgram;
        private int vao;
        private int vertexCount;

        private float time; // for wave animation

        public Game(int w, int h)
        {
            width = w;
            height = h;
        }

        // === Shader Sources ===
        private string vertexShaderCode = @"
            #version 330 core
            layout(location = 0) in vec3 aPosition;
            layout(location = 1) in vec3 aColor;

            uniform mat4 uMVP;
            uniform float uTime;

            out vec3 vColor;

            void main() 
            {
                // animate height with sine wave
                float wave = 0.05 * sin(10.0 * (aPosition.x + aPosition.z) + uTime);
                vec3 pos = vec3(aPosition.x, aPosition.y + wave, aPosition.z);

                gl_Position = uMVP * vec4(pos, 1.0);
                vColor = aColor;
            }
        ";

        private string fragmentShaderCode = @"
            #version 330 core
            in vec3 vColor;
            out vec4 FragColor;

            void main()
            {
                FragColor = vec4(vColor, 1.0);
            }
        ";

        // === Shader Utils ===
        private int CompileShader(string code, ShaderType type)
        {
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, code);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
                Console.WriteLine(GL.GetShaderInfoLog(shader));

            return shader;
        }

        private int CreateProgram(string vsCode, string fsCode)
        {
            int vs = CompileShader(vsCode, ShaderType.VertexShader);
            int fs = CompileShader(fsCode, ShaderType.FragmentShader);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vs);
            GL.AttachShader(program, fs);
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
                Console.WriteLine(GL.GetProgramInfoLog(program));

            GL.DeleteShader(vs);
            GL.DeleteShader(fs);

            return program;
        }

        public void Init()
        {
            shaderProgram = CreateProgram(vertexShaderCode, fragmentShaderCode);

            // Generate terrain mesh (512x512 grid)
            int gridSize = 128; // try 128 for testing (512 is heavier)
            var verts = new List<float>();

            for (int z = 0; z < gridSize - 1; z++)
            {
                for (int x = 0; x < gridSize - 1; x++)
                {
                    // two triangles per quad
                    AddVertex(verts, x, z, gridSize);
                    AddVertex(verts, x + 1, z, gridSize);
                    AddVertex(verts, x, z + 1, gridSize);

                    AddVertex(verts, x + 1, z, gridSize);
                    AddVertex(verts, x + 1, z + 1, gridSize);
                    AddVertex(verts, x, z + 1, gridSize);
                }
            }

            vertexCount = verts.Count / 6;

            vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Count * sizeof(float), verts.ToArray(), BufferUsageHint.StaticDraw);

            // position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // color
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);

            GL.ClearColor(0.1f, 0.2f, 0.3f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
        }

        private void AddVertex(List<float> verts, float x, float z, int gridSize)
        {
            float fx = (x / (float)gridSize) * 2f - 1f;
            float fz = (z / (float)gridSize) * 2f - 1f;
            float y = 0f;

            // position
            verts.Add(fx);
            verts.Add(y);
            verts.Add(fz);

            // gradient color
            verts.Add(x / (float)gridSize);
            verts.Add(0.5f);
            verts.Add(z / (float)gridSize);
        }

        public void Tick()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            time += 0.01f;

            GL.UseProgram(shaderProgram);

            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45f), width / (float)height, 0.1f, 100f);
            Matrix4 view = Matrix4.LookAt(new Vector3(1.5f, 2.5f, 2.5f), Vector3.Zero, Vector3.UnitY);
            Matrix4 model = Matrix4.Identity;
            Matrix4 mvp = model * view * proj;

            int mvpLoc = GL.GetUniformLocation(shaderProgram, "uMVP");
            GL.UniformMatrix4(mvpLoc, false, ref mvp);

            int timeLoc = GL.GetUniformLocation(shaderProgram, "uTime");
            GL.Uniform1(timeLoc, time);

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, vertexCount);
        }

        public void Cleanup()
        {
            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
        }
    }
}
