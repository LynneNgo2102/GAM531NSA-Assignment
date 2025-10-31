using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


namespace EndlessHallway
{
    public class Shader : IDisposable
    {
        public int Handle;
        public Shader(string vertPath, string fragPath)
        {
            var vs = File.ReadAllText(vertPath);
            var fs = File.ReadAllText(fragPath);
            int v = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(v, vs);
            GL.CompileShader(v);
            GL.GetShader(v, ShaderParameter.CompileStatus, out int ok);
            if (ok == 0) throw new Exception(GL.GetShaderInfoLog(v));


            int f = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(f, fs);
            GL.CompileShader(f);
            GL.GetShader(f, ShaderParameter.CompileStatus, out ok);
            if (ok == 0) throw new Exception(GL.GetShaderInfoLog(f));


            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, v);
            GL.AttachShader(Handle, f);
            GL.LinkProgram(Handle);
            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out ok);
            if (ok == 0) throw new Exception(GL.GetProgramInfoLog(Handle));


            GL.DetachShader(Handle, v);
            GL.DetachShader(Handle, f);
            GL.DeleteShader(v);
            GL.DeleteShader(f);
        }
        public void Use() => GL.UseProgram(Handle);
        public void SetMatrix4(string name, Matrix4 mat)
        {
            int loc = GL.GetUniformLocation(Handle, name);
            GL.UniformMatrix4(loc, false, ref mat);
        }
        public void SetVector3(string name, Vector3 v)
        {
            int loc = GL.GetUniformLocation(Handle, name);
            GL.Uniform3(loc, v);
        }
        public void SetFloat(string name, float f)
        {
            int loc = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(loc, f);
        }
        public void SetInt(string name, int i)
        {
            int loc = GL.GetUniformLocation(Handle, name);
            GL.Uniform1(loc, i);
        }
        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}