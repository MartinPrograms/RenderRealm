using Hexa.NET.OpenGL;

namespace RenderRealm.Graphics.Shaders;

public class UBO : IBuffer
{
    public uint Id { get; set; }
    
    public int Size { get; set; }
    public int Stride { get; set; }
    public Dictionary<string, int> Offsets { get; set; }
    public Dictionary<string, int> Sizes { get; set; }

    public void Construct(ShaderReflectionData reflectionData)
    {
        var gl = GraphicsContext.GL;
        Id = gl.GenBuffer();

        if (reflectionData.Blocks[0].Size > 0)
        {
            // This means it exists.
            Console.WriteLine($"UBO {reflectionData.Blocks[0].Name} exists with size {reflectionData.Blocks[0].Size}");
        }
        
        Bind();
    }

    public void Bind()
    {
        var gl = GraphicsContext.GL;
        gl.BindBuffer(GLBufferTargetARB.UniformBuffer, Id);
    }

    public void Unbind()
    {
        var gl = GraphicsContext.GL;
        gl.BindBuffer(GLBufferTargetARB.UniformBuffer, 0);
    }

    public unsafe void SetData(void* data, int size)
    {
        var gl = GraphicsContext.GL;
        gl.BufferData(GLBufferTargetARB.UniformBuffer, size, data, GLBufferUsageARB.StaticDraw);
    }

    public unsafe void SetSubData(void* data, int size, int offset)
    {
        var gl = GraphicsContext.GL;
        gl.BufferSubData(GLBufferTargetARB.UniformBuffer, offset, size, data);
    }
}

public unsafe interface IBuffer
{
    uint Id { get; set; }
    
    public void Construct(ShaderReflectionData data);
    public void Bind();
    public void Unbind();
    public void SetData(void* data, int size);
    public void SetSubData(void* data, int size, int offset); // Set a specific part of the buffer
}