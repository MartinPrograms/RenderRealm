using System.Numerics;
using Hexa.NET.ImGui;
using Hexa.NET.SDL3;
using Hexa.NET.Shaderc;
using RenderRealm;
using RenderRealm.Graphics;
using RenderRealm.Graphics.Abstractions;
using RenderRealm.Graphics.Common;
using RenderRealm.Graphics.Shaders;

var window = new Window("RenderRealm", 800, 600, SDLWindowFlags.Resizable | SDLWindowFlags.Maximized, 4,6);

window.SetVSync(1);
var gl = window.GL;

// How we create a shader without ANY fancy tools like SpirV or ShaderGen
// This is used on platforms where we can not use spirv or shadergen
var renderer = new DisplayShader(300,300,new ShaderStage(File.ReadAllText("../../../Shaders/Test.vert"), ShaderType.Vertex), new ShaderStage(File.ReadAllText("../../../Shaders/Test.frag"), ShaderType.Fragment));

renderer.SetUniforms += () =>
{
    
};

window.Render += () =>
{
    ImGui.Begin("Hello, world!");
    ImGui.Text("DT: " + Time.DeltaTime.ToString("0.000") + "s / " + (1.0 / Time.DeltaTime).ToString("0.0") + "fps");
    ImGui.End();
    
    ImGui.Begin("ShaderGen");
    ImGui.Text("This is some useful text.");
    var size = ImGui.GetContentRegionAvail();
    renderer.Resize((int)size.X, (int)size.Y);
    ImGui.Image(renderer.FrameBuffer.TextureId, size, new Vector2(0, 1), new Vector2(1, 0));
    ImGui.End();
    
    if (Input.IsKeyReleased(SDLScancode.F))
    {
        
    }
    
    renderer.Render();
    
};

window.Run();