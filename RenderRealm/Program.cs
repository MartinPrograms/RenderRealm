using System.Numerics;
using Hexa.NET.ImGui;
using Hexa.NET.SDL3;
using Hexa.NET.Shaderc;
using RenderRealm;
using RenderRealm.Graphics;
using RenderRealm.Graphics.Common;
using RenderRealm.Graphics.Shaders;
using RenderRealm.Shaders;

var window = new Window("RenderRealm", 800, 600, SDLWindowFlags.Resizable, 4,6);
window.SetVSync(1);
var gl = window.GL;

// How we create a shader without ANY fancy tools like SpirV or ShaderGen
// This is used on platforms where we can not use spirv or shadergen
var shader = new Shader(new ShaderStage(File.ReadAllText("../../../Shaders/Test.vert"), ShaderType.Vertex), new ShaderStage(File.ReadAllText("../../../Shaders/Test.frag"), ShaderType.Fragment));

var renderer = new ShaderRenderer(shader, Mesh.Square, 300,300);

Complex pos = new (-0.5f, 0);
double zoom = 1.0;
var iterations = 500;
Vector3 posLight = new Vector3(0.5f, 0.5f, 1f);

var crefOrbits = new Complex[500];

void changeCrefArray()
{
    // Use the mandelbrot iteration Zn+1 = Zn^2 + C to calculate the orbit of the complex number C
    var cref = new Complex(pos.Real, pos.Imaginary);
    cref = MandelbrotUtilities.CRef(zoom, cref);
    var z = new Complex(0, 0);
    for (int i = 0; i < crefOrbits.Length; i++)
    {
        z = z * z + cref;
        crefOrbits[i] = z;
    }
    
}

renderer.SetUniforms += () =>
{
    var cref = new Complex(pos.Real, pos.Imaginary);
    cref = MandelbrotUtilities.CRef(zoom, cref);
    
    changeCrefArray();
    
    renderer.Shader.SetUniform("cref", new Vector2((float)cref.Real, (float)cref.Imaginary)); 
    renderer.Shader.SetArray("crefOrbit", crefOrbits);
    
    renderer.Shader.SetUniform("pos", pos);
    renderer.Shader.SetUniform("zoom", zoom);
    renderer.Shader.SetUniform("iterations", iterations);
    renderer.Shader.SetUniform("u_time", (float)Time.TotalTime);
    renderer.Shader.SetUniform("u_resolution", new Vector2(renderer.FrameBuffer.Width, renderer.FrameBuffer.Height));
    
    renderer.Shader.SetUniform("posLight", posLight);
};

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
    
    if (Input.IsKeyReleased(SDLScancode.F))
    {
        // Reload the shader
        shader.Dispose();
        shader = new Shader(new ShaderStage(File.ReadAllText("../../../Shaders/Test.vert"), ShaderType.Vertex), new ShaderStage(File.ReadAllText("../../../Shaders/Test.frag"), ShaderType.Fragment));
        renderer.Shader = shader;
    }
    
    if (Input.IsKeyDown(SDLScancode.W))
    {
        pos = new Complex(pos.Real, pos.Imaginary + 0.01f * zoom);
    }
    
    if (Input.IsKeyDown(SDLScancode.S))
    {
        pos = new Complex(pos.Real, pos.Imaginary - 0.01f * zoom);
    }
    
    if (Input.IsKeyDown(SDLScancode.A))
    {
        pos = new Complex(pos.Real - 0.01f * zoom, pos.Imaginary);
    }
    
    if (Input.IsKeyDown(SDLScancode.D))
    {
        pos = new Complex(pos.Real + 0.01f * zoom, pos.Imaginary);
    }
    
    if (Input.IsKeyDown(SDLScancode.Q))
    {
        zoom *= 1.01f;
    }
    
    if (Input.IsKeyDown(SDLScancode.E))
    {
        zoom *= 0.99f;
    }
    
    if (Input.IsKeyDown(SDLScancode.Right))
    {
        posLight.X += 0.01f;
    }
    
    if (Input.IsKeyDown(SDLScancode.Left))
    {
        posLight.X -= 0.01f;
    }
    
    if (Input.IsKeyDown(SDLScancode.Up))
    {
        posLight.Y += 0.01f;
    }
    
    if (Input.IsKeyDown(SDLScancode.Down))
    {
        posLight.Y -= 0.01f;
    }
    
    if (Input.IsKeyDown(SDLScancode.Rshift))
    {
        posLight.Z += 0.01f;
    }
    
    if (Input.IsKeyDown(SDLScancode.Rctrl))
    {
        posLight.Z -= 0.01f;
    }
    
    renderer.Render();
    
};

window.Run();