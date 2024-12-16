using RenderRealm.Graphics.Common;
using RenderRealm.Graphics.Shaders;

namespace RenderRealm.Graphics.Abstractions;

public class DisplayShader
{
    ShaderRenderer _shaderRenderer;
    
    public Action SetUniforms
    {
        get => _shaderRenderer.SetUniforms;
        set => _shaderRenderer.SetUniforms = value;
    }
    
    public Shader Shader => _shaderRenderer.Shader;
    public Mesh Mesh => _shaderRenderer.Mesh;
    public FrameBuffer FrameBuffer => _shaderRenderer.FrameBuffer;
    
    public void Resize(int sizeX, int sizeY)
    {
        _shaderRenderer.Resize(sizeX, sizeY);
    }
    
    public void SetUniform<T>(string name, T value) => _shaderRenderer.Shader.SetUniform(name, value);
    public void SetArray<T>(string name, T[] value) => _shaderRenderer.Shader.SetArray(name, value);
    
    public DisplayShader(int width, int height, params ShaderStage[] stages)
    {
        _shaderRenderer = new ShaderRenderer(new Shader(stages), Mesh.Square, width, height);
    }
    
    public void Render()
    {
        _shaderRenderer.Render();
    }

    public void Reload()
    {
        _shaderRenderer.Shader.Reload();
    }
}