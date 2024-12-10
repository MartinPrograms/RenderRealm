using System.Numerics;
using System.Runtime.CompilerServices;
using Hexa.NET.ImGui;
using Hexa.NET.SDL3;
using Hexa.NET.OpenGL;
using Hexa.NET.OpenGL;
using HexaGen.Runtime;
using RenderRealm.Graphics.Common;
using Extensions = RenderRealm.Graphics.Common.Extensions;
using SDLEvent = Hexa.NET.SDL3.SDLEvent;
using SDLWindow = Hexa.NET.SDL3.SDLWindow;

namespace RenderRealm.Graphics;

public unsafe class Window
{
    public string Name
    {
        get { return Extensions.FromPointer(SDL.GetWindowTitle(Handle)); }
        set { SDL.SetWindowTitle(Handle, value); }
    }

    public unsafe int Width
    {
        get
        {
            int w = 0;
            int h = 0;
            SDL.GetWindowSize(Handle, &w, &h);
            return w;
        }
        
        set { SDL.SetWindowSize(Handle, (int)value, (int)Height); }
    }

    public int Height 
    {
        get
        {
            int w = 0;
            int h = 0;
            SDL.GetWindowSize(Handle, &w, &h);
            return h;
        }
        
        set { SDL.SetWindowSize(Handle, (int)Width, (int)value); }
    }

    public SDLWindow* Handle { get; }
    public SDLGLContext Context { get; }
    public uint ID { get; }
    
    public GL GL { get; }
    
    private ImGuiController? _imguiController;
    
    public Window(string name, int width, int height, SDLWindowFlags flags, int major, int minor)
    {
        Name = name;

        SDL.SetHint(SDL.SDL_HINT_MOUSE_FOCUS_CLICKTHROUGH, "1");
        SDL.SetHint(SDL.SDL_HINT_OPENGL_LIBRARY, "opengl32");
        SDL.SetHint(SDL.SDL_HINT_RENDER_DRIVER, "opengl");
        SDL.SetHint(SDL.SDL_HINT_VIDEO_X11_NET_WM_BYPASS_COMPOSITOR, "0");
        SDL.SetHint(SDL.SDL_HINT_VIDEO_X11_NET_WM_PING, "0");
        
        SDL.GLSetAttribute(SDLGLattr.GlDoublebuffer, 1);
        SDL.GLSetAttribute(SDLGLattr.GlDepthSize, 24);
        SDL.GLSetAttribute(SDLGLattr.GlStencilSize, 8);
        SDL.GLSetAttribute(SDLGLattr.GlContextFlags, (int)SDLGLcontextFlag.GlContextDebugFlag);

        if (major < 4 || minor < 6)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Forward compatibility is not supported with OpenGL 4.6 or lower.");
            Console.WriteLine("Spir-V and ShaderC are now disabled, and will return the default error shader if used.");
            Console.ResetColor();
            SDL.GLSetAttribute(SDLGLattr.GlContextFlags, (int)SDLGLcontextFlag.GlContextForwardCompatibleFlag);
            
            GraphicsContext.CompatibleSpirV = false;
        }
        
        SDL.GLSetAttribute(SDLGLattr.GlContextMajorVersion, major);
        SDL.GLSetAttribute(SDLGLattr.GlContextMinorVersion, minor);
        SDL.GLSetAttribute(SDLGLattr.GlContextProfileMask, (int)SDLGLprofile.GlContextProfileCore);

        SDL.Init(SDLInitFlags.Events | SDLInitFlags.Video);

        Handle = SDL.CreateWindow(name, width, height, flags | SDLWindowFlags.Opengl); // Yes we force OpenGL, what are you going to do about it? Fight me!
        ID = SDL.GetWindowID(Handle);
        
        Height = height;
        Width = width;
        
        Context = SDL.GLCreateContext(Handle);
        CheckSdlError(Context.Handle != IntPtr.Zero, "Failed to create GL context");
        CheckSdlError(SDL.GLMakeCurrent(Handle, Context), "Failed to make GL context current");
        
        CheckSdlError(SDL.GLSetSwapInterval(1), "Failed to set swap interval");

        GL = new GL(new GLContext(this));
        GraphicsContext.SetGL(GL);
        
        _imguiController = new ImGuiController(this);
        
        MouseMove += (@event, x, y) => Input.UpdateMousePosition(x, y);
        MouseButton += @event => Input.UpdateMouseButton(@event);
        KeyDown += @event => Input.UpdateKeyDown(@event);
        KeyUp += @event => Input.UpdateKeyUp(@event);
    }

    public Action Load { get; set; }
    public Action Update { get; set; }
    public Action Render { get; set; }
    public Action Close { get; set; }
    private bool running = true;
    public void Run()
    {
        Load?.Invoke();
        
        SDLEvent sdlEvent = default!;
        while (running)
        {
            Time.Update();
            
            GraphicsContext.Width = Width;
            GraphicsContext.Height = Height;
            
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(GLClearBufferMask.ColorBufferBit);
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            
            _imguiController?.SetResolution(Width, Height);
            _imguiController?.NewFrame();
            
            while (SDL.PollEvent(ref sdlEvent))
            {
                running = PollMethod(sdlEvent, running);
            }

            Update?.Invoke();
            Render?.Invoke();
            
            _imguiController?.Render();
            
            SDL.GLSwapWindow(Handle);
            Input.Update();
        }
        
        Close?.Invoke();
        
        _imguiController?.Shutdown();
        
        SDL.GLDestroyContext(Context);
        SDL.DestroyWindow(Handle);
        SDL.Quit();
    }

    private bool PollMethod(SDLEvent sdlEvent, bool running)
    {
        switch ((SDLEventType)sdlEvent.Type)
        {
            case SDLEventType.Quit:
                running = false;
                break;

            case SDLEventType.Terminating:
                running = false;
                break;

            case SDLEventType.WindowCloseRequested:
                var windowEvent = sdlEvent.Window;
                if (windowEvent.WindowID == ID)
                {
                    running = false;
                }

                break;
                    
            case SDLEventType.MouseWheel:
                var wheelEvent = sdlEvent.Wheel;
                MouseWheel?.Invoke(wheelEvent, wheelEvent.X, wheelEvent.Y);
                break;
                    
            case SDLEventType.MouseMotion:
                var motionEvent = sdlEvent.Motion;
                MouseMove?.Invoke(motionEvent, motionEvent.X, motionEvent.Y);
                break;
                    
            case SDLEventType.MouseButtonDown:
                var buttonEvent = sdlEvent.Button;
                MouseButton?.Invoke(buttonEvent);
                break;
                    
            case SDLEventType.MouseButtonUp:
                var buttonUpEvent = sdlEvent.Button;
                MouseButton?.Invoke(buttonUpEvent);
                break;
                    
            case SDLEventType.KeyDown:
                var keyDownEvent = sdlEvent.Key;
                KeyDown?.Invoke(keyDownEvent);
                break;
                    
            case SDLEventType.KeyUp:
                var keyUpEvent = sdlEvent.Key;
                KeyUp?.Invoke(keyUpEvent);
                break;
                    
            default:
                break; // Ignore all other events
        }

        return running;
    }

    public static void CheckSdlError(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception($"{message}: {SDL.GetError()->ToString()}");
        }
    }

    public event Action<SDLMouseWheelEvent, float, float> MouseWheel;
    public event Action<SDLMouseMotionEvent, float, float> MouseMove;
    public Action<SDLMouseButtonEvent> MouseButton;
    
    public Action<SDLKeyboardEvent> KeyDown;
    public Action<SDLKeyboardEvent> KeyUp;

    public void SetVSync(int i)
    {
        SDL.GLSetSwapInterval(i);
    }

    public void Exit()
    {
        running = false;
    }
}

public static class Time
{
    public static double DeltaTime { get; private set; }
    public static double TotalTime { get; private set; }
    
    private static double _lastFrameTime;
    public static void Update()
    {
        double currentFrameTime = (double)SDL.GetTicks() / 1000.0;
        DeltaTime = currentFrameTime - _lastFrameTime;
        _lastFrameTime = currentFrameTime;
        TotalTime += DeltaTime;    
    }
}

public static class GraphicsContext
{
    public static GL GL { get; private set; }
    public static int Width { get; set; }
    public static int Height { get; set; }
    
    public static bool CompatibleSpirV { get; set; } = true;

    public static void SetGL(GL gl)
    {
        GL = gl;
    }
}