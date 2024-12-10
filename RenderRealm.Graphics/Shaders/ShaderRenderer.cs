using RenderRealm.Graphics.Common;

namespace RenderRealm.Graphics.Shaders;

public class ShaderRenderer : IDisposable, IRenderer
{
    public Shader Shader { get; set; }
    public Mesh Mesh { get; set; }
    public FrameBuffer FrameBuffer { get; set; }
    
    public ShaderRenderer(Shader shader, Mesh mesh, int width, int height)
    {
        Shader = shader;
        Mesh = mesh;
        FrameBuffer = new FrameBuffer(width, height);
    }
    
    public void Render()
    {
        FrameBuffer.Bind();
        Shader.Use();
        Mesh.Render();
        FrameBuffer.Unbind();
    }
    
    private void ReleaseUnmanagedResources()
    {
        FrameBuffer.Dispose();
        Shader.Dispose();
        Mesh.Dispose();
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~ShaderRenderer()
    {
        ReleaseUnmanagedResources();
    }

    public void Resize(int sizeX, int sizeY)
    {
        FrameBuffer.Resize(sizeX, sizeY);
    }
}