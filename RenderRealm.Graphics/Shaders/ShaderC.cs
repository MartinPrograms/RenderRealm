using System.Numerics;
using Hexa.NET.OpenGL;
using Hexa.NET.Shaderc;
using RenderRealm.Graphics.Common;
using Extensions = RenderRealm.Graphics.Common.Extensions;

namespace RenderRealm.Graphics.Shaders;

/// <summary>
/// Almost 1:1 to a regular shader, however this one uses spirv instead of regular GLSL
/// </summary>
public class ShaderC : IShader
{
    public uint Handle { get; private set; }
    public List<ShaderStage> Stages { get; private set; }
    
    private ShaderReflectionData _vertexReflection;
    private ShaderReflectionData _fragmentReflection;
    
    private UBO _vertexUniformBuffer;
    private UBO _fragmentUniformBuffer;
    
    public ShaderC(string vertPath, string fragPath)
    {
        var shaderc = new ShaderCManager(ShaderCManager.DefaultOptions);
        var vert = shaderc.Compile(File.ReadAllText(vertPath), vertPath, ShadercShaderKind.VertexShader, "main", out _vertexReflection);
        var frag = shaderc.Compile(File.ReadAllText(fragPath), fragPath, ShadercShaderKind.FragmentShader, "main", out _fragmentReflection);
        
        if (vert.HasValue && frag.HasValue)
        {
            Stages = new List<ShaderStage>
            {
                new ShaderStage(vert.Value, ShaderType.Vertex),
                new ShaderStage(frag.Value, ShaderType.Fragment)
            };
         
            _vertexUniformBuffer = new UBO();
            _vertexUniformBuffer.Construct(_vertexReflection);
            
            _fragmentUniformBuffer = new UBO();
            _fragmentUniformBuffer.Construct(_fragmentReflection);
            
            
            
            Compile();
        }
        else
        {
            Stages = new List<ShaderStage>();
        }
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

        if (vertexOnly)
        {
            
        }
        else
        {
            
        }
    }

}