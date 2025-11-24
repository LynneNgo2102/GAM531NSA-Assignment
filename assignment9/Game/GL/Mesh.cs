using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


namespace BackRoomMap
{
    public class Mesh : IDisposable
    {
        int vao, vbo, ebo;
        int indexCount;

        public Mesh(float[] verts, int[] idx)
        {
            // Your existing VAO/VBO/EBO binding and setup logic here
            // Ensure the stride is correct: 8 * sizeof(float) (3 pos, 3 normal, 2 tex)
            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();
            ebo = GL.GenBuffer();
            indexCount = idx.Length;

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, idx.Length * sizeof(int), idx, BufferUsageHint.StaticDraw);

            int stride = 8 * sizeof(float); // pos(3) normal(3) tex(2)
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, 3 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

            GL.BindVertexArray(0);
        }

        public void Render()
        {
            GL.BindVertexArray(vao);
            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
        }

        public void Dispose()
        {
            GL.DeleteBuffer(vbo);
            GL.DeleteBuffer(ebo);
            GL.DeleteVertexArray(vao);
        }

        // Helper: create simple cube (Your existing data is good)
        public static Mesh CreateCube()
        {
            // Vertices and indices are the same as before. 
            // They represent an outward-facing cube centered at the origin.
            // (12 triangles, 36 indices, 24 vertices)
            float[] vertices = { /*3 pos, 3 normal, 2 tex for 24 vertices  */ 
                // Front face (+Z)
                -0.5f, -0.5f, 0.5f, 0,0,1, 0,0,
                0.5f, -0.5f, 0.5f, 0,0,1, 1,0,
                0.5f, 0.5f, 0.5f, 0,0,1, 1,1,
                -0.5f, 0.5f, 0.5f, 0,0,1, 0,1,
                // other 5 faces, 4 vertices each
                // Back face (-Z)
                -0.5f, -0.5f, -0.5f, 0,0,-1, 0,0,
                0.5f, -0.5f, -0.5f, 0,0,-1, 1,0,
                0.5f, 0.5f, -0.5f, 0,0,-1, 1,1,
                -0.5f, 0.5f, -0.5f, 0,0,-1, 0,1,
                // Left (-X)
                -0.5f, -0.5f, -0.5f, -1,0,0, 0,0,
                -0.5f, -0.5f, 0.5f, -1,0,0, 1,0,
                -0.5f, 0.5f, 0.5f, -1,0,0, 1,1,
                -0.5f, 0.5f, -0.5f, -1,0,0, 0,1,
                // Right (+X)
                0.5f, -0.5f, -0.5f, 1,0,0, 0,0,
                0.5f, -0.5f, 0.5f, 1,0,0, 1,0,
                0.5f, 0.5f, 0.5f, 1,0,0, 1,1,
                0.5f, 0.5f, -0.5f, 1,0,0, 0,1,
                // Top (+Y)
                -0.5f, 0.5f, 0.5f, 0,1,0, 0,0,
                0.5f, 0.5f, 0.5f, 0,1,0, 1,0,
                0.5f, 0.5f, -0.5f, 0,1,0, 1,1,
                -0.5f, 0.5f, -0.5f, 0,1,0, 0,1,
                // Bottom (-Y)
                -0.5f, -0.5f, 0.5f, 0,-1,0, 0,0,
                0.5f, -0.5f, 0.5f, 0,-1,0, 1,0,
                0.5f, -0.5f, -0.5f, 0,-1,0, 1,1,
                -0.5f, -0.5f, -0.5f, 0,-1,0, 0,1,
            };

            int[] indices = {
                0,1,2, 2,3,0,
                4,5,6, 6,7,4,
                8,9,10, 10,11,8,
                12,13,14, 14,15,12,
                16,17,18, 18,19,16,
                20,21,22, 22,23,20
            };

            return new Mesh(vertices, indices);
        }

        // mesh2: Floor/Ceiling Quad (Normal pointing up: +Y) 
        public static Mesh CreateYQuad()
        {
            float[] verts = {
                // pos            normal      tex
                -0.5f, 0.0f, 0.5f, 0, 1, 0, 0, 0,
                 0.5f, 0.0f, 0.5f, 0, 1, 0, 1, 0,
                 0.5f, 0.0f, -0.5f, 0, 1, 0, 1, 1,
                -0.5f, 0.0f, -0.5f, 0, 1, 0, 0, 1,
            };
            int[] idx = { 0, 1, 2, 2, 3, 0 };
            return new Mesh(verts, idx);
        }

        // mesh3:  Wall Quad (Normal pointing front: +Z) 
        public static Mesh CreateZQuad()
        {
            float[] verts = {
                // pos            normal      tex
                -0.5f, -0.5f, 0.0f, 0, 0, 1, 0, 0,
                 0.5f, -0.5f, 0.0f, 0, 0, 1, 1, 0,
                 0.5f, 0.5f, 0.0f, 0, 0, 1, 1, 1,
                -0.5f, 0.5f, 0.0f, 0, 0, 1, 0, 1,
            };
            int[] idx = { 0, 1, 2, 2, 3, 0 };
            return new Mesh(verts, idx);
        }


    }
}