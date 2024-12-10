using Hexa.NET.SDL3;
using HexaGen.Runtime;

namespace RenderRealm.Graphics.Common;

public class GLContext : IGLContext
{
    public unsafe GLContext(Window window)
    {
        this.Handle = window.Context.Handle;
        this.IsCurrent = true;
        
        this.window = window.Handle;
    }
    
    public void Dispose()
    {
        this.IsCurrent = false;
    }

    public unsafe IntPtr GetProcAddress(string procName)
    {
        return (IntPtr) SDL.GLGetProcAddress(procName);
    }

    public bool TryGetProcAddress(string procName, out IntPtr procAddress)
    {
        procAddress = GetProcAddress(procName);
        return procAddress != IntPtr.Zero;
    }

    public bool IsExtensionSupported(string extensionName)
    {
        return SDL.GLExtensionSupported(extensionName);
    }

    public void MakeCurrent()
    {
        this.IsCurrent = true;
    }

    public unsafe void SwapBuffers()
    {
        SDL.GLSwapWindow(this.window);
    }

    public void SwapInterval(int interval)
    {
        SDL.GLSetSwapInterval(interval);
    }

    private readonly unsafe SDLWindow *window;
    public IntPtr Handle { get; private set; }
    public bool IsCurrent { get; private set; }
}