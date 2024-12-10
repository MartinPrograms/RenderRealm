using System.Numerics;
using Hexa.NET.ImGui;
using Hexa.NET.SDL3;
using Hexa.NET.Shaderc;
using RenderRealm.Graphics;
using RenderRealm.Graphics.Common;
using RenderRealm.Graphics.Shaders;
using RenderRealm.Shaders;

var window = new Window("RenderRealm", 800, 600, SDLWindowFlags.Resizable, 4,6);
window.SetVSync(1);
var gl = window.GL;

// How we create a shader without ANY fancy tools like SpirV or ShaderGen
// This is used on platforms where we can not use spirv or shadergen
var shader = new Shader(new ShaderStage(File.ReadAllText("Shaders/Test.vert"), ShaderType.Vertex), new ShaderStage(File.ReadAllText("Shaders/Test.frag"), ShaderType.Fragment));

var shaderc = new ShaderCManager(ShaderCManager.DefaultOptions);
var vert = shaderc.Compile(File.ReadAllText("../../../Shaders/SpirV/Test.vert"), "../../../Shaders/SpirV/Test.vert", ShadercShaderKind.VertexShader, "main", out var vertReflection);
var frag = shaderc.Compile(File.ReadAllText("../../../Shaders/SpirV/Test.frag"), "../../../Shaders/SpirV/Test.frag", ShadercShaderKind.FragmentShader, "main", out var fragReflection);

Shader? coolShader = null;
if (vert.HasValue && frag.HasValue)
{
    coolShader = new Shader(new []{new ShaderStage(vert.Value, ShaderType.Vertex), new ShaderStage(frag.Value, ShaderType.Fragment)});
}
else
{
    coolShader = Shader.ErrorShader;
}

var renderer = new ShaderRenderer(coolShader, Mesh.Square, 300,300);

window.Render += () =>
{
    ImGui.Begin("Hello, world!");
    ImGui.Text("This is some useful text.");
    ImGui.Text("DT: " + Time.DeltaTime.ToString("0.000") + "s / " + (1.0 / Time.DeltaTime).ToString("0.0") + "fps");
    ImGui.End();
    
    
    ImGui.Begin("ShaderGen");
    ImGui.Text("This is some useful text.");
    var size = ImGui.GetContentRegionAvail();
    renderer.Resize((int)size.X, (int)size.Y);
    ImGui.Image(renderer.FrameBuffer.TextureId, size, new Vector2(0, 1), new Vector2(1, 0));
    ImGui.End();
    
    if (Input.IsKeyDown(SDLScancode.F))
    {
        vert = shaderc.Compile(File.ReadAllText("../../../Shaders/SpirV/Test.vert"), "../../../Shaders/SpirV/Test.vert", ShadercShaderKind.VertexShader, "main", out vertReflection);
        frag = shaderc.Compile(File.ReadAllText("../../../Shaders/SpirV/Test.frag"), "../../../Shaders/SpirV/Test.frag", ShadercShaderKind.FragmentShader, "main", out fragReflection);
        renderer.Shader = new Shader(new []{new ShaderStage(vert.Value, ShaderType.Vertex), new ShaderStage(frag.Value, ShaderType.Fragment)});
    }
    
    renderer.Render();
    
};

window.Run();