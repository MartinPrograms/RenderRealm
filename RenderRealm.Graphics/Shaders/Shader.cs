using System.Numerics;
using System.Runtime.InteropServices;
using Hexa.NET.OpenGL;
using Hexa.NET.Shaderc;
using RenderRealm.Graphics.Common;
using Extensions = RenderRealm.Graphics.Common.Extensions;

namespace RenderRealm.Graphics.Shaders;

public class Shader : IShader
{
    public uint Handle { get; private set; }
    public List<ShaderStage> Stages { get; private set; }
    // Because we are epic we will use ShaderGen to generate our shaders, however thats not done in this class
 
    public Shader(params ShaderStage[] stages)
    {
        if (!_createdErrorShader)
        {
            CreateErrorShader();
        }

        Stages = new List<ShaderStage>(stages);
        
        Compile();
    }


    public unsafe bool Compile()
    {
        var gl = GraphicsContext.GL;
        Handle = gl.CreateProgram();
        
        int len = 0;
        var log = new byte[256];
        bool success = true;
        foreach (var stage in Stages)
        {
            var shader = gl.CreateShader(stage.Type.ToGLShaderType());
            if (!stage.SpirV)
            {
                gl.ShaderSource(shader, stage.Source);
                gl.CompileShader(shader);
                
                gl.GetShaderInfoLog(shader, 256, &len, log);
                if (len > 0)
                {
                    Console.WriteLine($"Shader compilation error: {Extensions.FromArray(log)}");
                    success = false;
                }
            }
            else
            {
                fixed (void* ptr = stage.SourceB)
                {
                    gl.ShaderBinary(1, &shader, GLShaderBinaryFormat.SpirV, ptr, stage.SourceB.Length);
                }gl.SpecializeShader(shader, "main", 0, null, null);
                
                gl.GetShaderInfoLog(shader, 256, &len, log);
                if (len > 0)
                {
                    Console.WriteLine($"Shader compilation error (SpirV): {Extensions.FromArray(log)}");
                    success = false;
                }
            }
            
            gl.AttachShader(Handle, shader);
            gl.DeleteShader(shader);
        }
        
        gl.LinkProgram(Handle);
        gl.GetProgramInfoLog(Handle, 256, &len, log);
        
        if (len > 0)
        {
            Console.WriteLine($"Shader linking error: {Extensions.FromArray(log)}");
            success = false;
        }
        
        Console.WriteLine($"Shader compilation {(success ? "succeeded" : "failed")}");
        
        return success;
    }
    
    public void Use()
    {
        GraphicsContext.GL.UseProgram(Handle);
    }
    
    public void Dispose()
    {
        GraphicsContext.GL.DeleteProgram(Handle);
    }
    
    public void SetUniform<T>(string name, T value, bool vertexOnly = false)
    {
        var gl = GraphicsContext.GL;
        var location = gl.GetUniformLocation(Handle, name);
        
        if (value is int i)
        {
            gl.Uniform1i(location, i);
        }
        else if (value is float f)
        {
            gl.Uniform1f(location, f);
        }
        else if (value is Vector2 v2)
        {
            gl.Uniform2f(location, v2.X, v2.Y);
        }
        else if (value is Vector3 v3)
        {
            gl.Uniform3f(location, v3.X, v3.Y, v3.Z);
        }
        else if (value is Vector4 v4)
        {
            gl.Uniform4f(location, v4.X, v4.Y, v4.Z, v4.W);
        }
        else if (value is Matrix4x4 m4)
        {
            gl.UniformMatrix4fv(location, 1, false, ref m4.M11);
        }
        else if (value is Complex c)
        {
            gl.Uniform2d(location, c.Real, c.Imaginary);
        }
        else if (value is double d)
        {
            gl.Uniform1d(location, d);
        }
        else
        {
            throw new ArgumentException("Invalid uniform type");
        }
    }

    public void SetArray<T>(string name, T[] value)
    {
        if (value.Length == 0)
        {
            return;
        }
        
        var gl = GraphicsContext.GL;
        var location = gl.GetUniformLocation(Handle, name);
        
        if (value[0] is int i)
        {
            gl.Uniform1iv(location, value.Length, value.Cast<int>().ToArray());
        }
        else if (value[0] is float f)
        {
            gl.Uniform1fv(location, value.Length, value.Cast<float>().ToArray());
        }
        else if (value[0] is Vector2 v2)
        {
            gl.Uniform2fv(location, value.Length, value.Cast<Vector2>().SelectMany(x => new[] {x.X, x.Y}).ToArray());
        }
        else if (value[0] is Vector3 v3)
        {
            gl.Uniform3fv(location, value.Length, value.Cast<Vector3>().SelectMany(x => new[] {x.X, x.Y, x.Z}).ToArray());
        }
        else if (value[0] is Vector4 v4)
        {
            gl.Uniform4fv(location, value.Length, value.Cast<Vector4>().SelectMany(x => new[] {x.X, x.Y, x.Z, x.W}).ToArray());
        }
        else if (value[0] is Complex c)
        {
            gl.Uniform2fv(location, value.Length, value.Cast<Complex>().SelectMany(x => new[] {(float)x.Real, (float)x.Imaginary}).ToArray());
        }
        else
        {
            throw new ArgumentException("Invalid uniform type");
        }
    }

    public static Shader ErrorShader = default!;
    public static bool _createdErrorShader = false;
    
    private static void CreateErrorShader()
    {
        _createdErrorShader = true;
        // Checkerboard shader, red and black
        ErrorShader = new Shader(new ShaderStage(@"#version 330 core

layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoord;

out vec2 TexCoord;

void main()
{
    gl_Position = vec4(aPos, 0.0, 1.0);

    TexCoord = aTexCoord;
}
", ShaderType.Vertex), new ShaderStage(@"#version 330 core

out vec4 FragColor;

in vec2 TexCoord;

void main()

{
// big X, red and black
    if (mod(gl_FragCoord.x, 20) < 10 && mod(gl_FragCoord.y, 20) < 10)
    {
        FragColor = vec4(1.0, 0.0, 0.0, 1.0);
    }
    else if (mod(gl_FragCoord.x, 20) < 10 && mod(gl_FragCoord.y, 20) >= 10)
    {
        FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    }
    else if (mod(gl_FragCoord.x, 20) >= 10 && mod(gl_FragCoord.y, 20) < 10)
    {
        FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    }
    else
    {
        FragColor = vec4(1.0, 0.0, 0.0, 1.0);
    }
}

", ShaderType.Fragment));
            
        Console.WriteLine("Error shader created");
    }

    public void Reload()
    {
        Dispose();
        foreach (var stage in Stages)
        {
            if (string.IsNullOrEmpty(stage.Path))
            {
                continue;
            }
            
            stage.Source = File.ReadAllText(stage.Path);
        }
    }
}

public unsafe class ShaderStage
{
    public string Source { get; set; }
    public string Path { get; set; }
    public byte[] SourceB { get; set; }
    public ShaderType Type { get; set; }
    public bool SpirV { get; }
    public ShaderStage(string source, ShaderType type, string path = "")
    {
        Source = source;
        Type = type;
        Path = path;
        SpirV = false;
    }
    
    public ShaderStage(ShadercCompilationResult result, ShaderType type)
    {
        Source = "This is a SpirV shader";

        var len = result.GetLength();
        var ptr = result.GetBytes(); // Pointer to the SpirV shader
        
        SourceB = new byte[len];
        Marshal.Copy(new IntPtr(ptr), SourceB, 0, ((int)len.ToUInt32()));
        
        Type = type;
        SpirV = true;
    }
}

public enum ShaderType
{
    Vertex,
    Fragment,
    Geometry,
    Compute,
    TessellationControl,
    TessellationEvaluation
}

public interface IShader
{
    public void Use();
    public void Dispose();
    public void SetUniform<T>(string name, T value, bool vertexOnly = false);
}