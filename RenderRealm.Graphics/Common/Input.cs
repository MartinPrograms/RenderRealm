using System.Numerics;
using Hexa.NET.SDL3;

namespace RenderRealm.Graphics.Common;

public static class Input
{
    private static List<SDLScancode> _keysDown = new();
    private static List<SDLScancode> _previousKeysDown = new();
    private static List<SDLScancode> _nextKeysDown = new();
    
    public static bool IsKeyDown(SDLScancode key) => _keysDown.Contains(key);
    public static bool IsKeyUp(SDLScancode key) => !_keysDown.Contains(key);
    public static bool IsKeyPressed(SDLScancode key) => _keysDown.Contains(key) && !_previousKeysDown.Contains(key);
    public static bool IsKeyReleased(SDLScancode key) => !_keysDown.Contains(key) && _previousKeysDown.Contains(key);
    
    private static Vector2 _mousePosition;
    private static Vector2 _previousMousePosition;
    private static Vector2 _nextMousePosition;
    private static Vector2 _mouseDelta => _mousePosition - _previousMousePosition;
    
    public static Vector2 MousePosition => _mousePosition;
    public static Vector2 MouseDelta => _mouseDelta;
    
    private static List<SDLMouseButtonEvent> _mouseButtonDown = new();
    private static List<SDLMouseButtonEvent> _previousMouseButtonDown = new();
    private static List<SDLMouseButtonEvent> _nextMouseButtonDown = new();

    public static bool IsMouseButtonDown(SDLMouseButtonFlags button) =>
        _mouseButtonDown.Any(x => (x.Button & (int)button) != 0);    
    
    public static void Update()
    {
        _previousKeysDown.Clear();
        _previousKeysDown.AddRange(_keysDown);
        
        _keysDown.Clear();
        _keysDown.AddRange(_nextKeysDown);
        
        _previousMousePosition = _mousePosition;
        _mousePosition = _nextMousePosition;
        
        _previousMouseButtonDown.Clear();
        _previousMouseButtonDown.AddRange(_mouseButtonDown);
        
        _mouseButtonDown.Clear();
        _mouseButtonDown.AddRange(_nextMouseButtonDown);
        
        _nextMouseButtonDown.Clear();
    }
    
    public static void UpdateMousePosition(Vector2 position)
    {
        _nextMousePosition = position;
    }
    
    public static void UpdateMousePosition(float x, float y)
    {
        _nextMousePosition = new Vector2(x, y);
    }
    
    public static void UpdateMouseButton(SDLMouseButtonEvent @event)
    {
        if (@event.Down == 1)
        {
            _nextMouseButtonDown.Add(@event);
        }
        else
        {
            _nextMouseButtonDown.RemoveAll(x => x.Button == @event.Button);
        }
    }
    
    public static void UpdateKeyDown(SDLKeyboardEvent @event)
    {
        _nextKeysDown.Add(@event.Scancode);
    }
    
    public static void UpdateKeyUp(SDLKeyboardEvent @event)
    {
        _nextKeysDown.RemoveAll(x => x == @event.Scancode);
    }
}