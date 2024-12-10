using System.Runtime.InteropServices;
using System.Text;
using Hexa.NET.OpenGL;
using RenderRealm.Graphics.Shaders;

namespace RenderRealm.Graphics.Common;

public static class Extensions
{
    public unsafe static string FromPointer(byte* ptr)
    {
        return Marshal.PtrToStringAnsi((IntPtr)ptr);
    }
    
    public unsafe static string FromArray(byte[] arr)
    {
        fixed (byte* ptr = arr)
        {
            return FromPointer(ptr);
        }
    }
    
    public unsafe static string FromPointer(byte* ptr, int length)
    {
        return Encoding.UTF8.GetString(ptr, length);
    }
    
    // To pointer
    public unsafe static byte* ToPointer(string str)
    {
        return (byte*)Marshal.StringToHGlobalAnsi(str).ToPointer();
    }
    
    public static int Limit(this int value, int min, int max)
    {
        return value < min ? min : value > max ? max : value;
    }
    
    public static long Limit(this long value, long min, long max)
    {
        return value < min ? min : value > max ? max : value;
    }
    
    public static GLShaderType ToGLShaderType(this ShaderType type)
    {
        return type switch
        {
            ShaderType.Vertex => GLShaderType.VertexShader,
            ShaderType.Fragment => GLShaderType.FragmentShader,
            ShaderType.Geometry => GLShaderType.GeometryShader,
            ShaderType.Compute => GLShaderType.ComputeShader,
            ShaderType.TessellationControl => GLShaderType.TessControlShader,
            ShaderType.TessellationEvaluation => GLShaderType.TessEvaluationShader,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}