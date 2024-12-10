using Hexa.NET.ImGui;
using Hexa.NET.ImGui.Backends.OpenGL3;
using Hexa.NET.SDL3;
using Hexa.NET.Utilities.Extensions;

namespace RenderRealm.Graphics.Common;

public class ImGuiController
{
    private readonly Window window;
    public ImGuiController(Window window)
    {
        this.window = window;
        
        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        
        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;     // Enable Keyboard Controls
        io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;      // Enable Gamepad Controls
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;         // Enable Docking
        io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;       // Enable Multi-Viewport / Platform Windows
        io.ConfigViewportsNoAutoMerge = false;
        io.ConfigViewportsNoTaskBarIcon = false;
        
        io.DisplaySize = new System.Numerics.Vector2(window.Width, window.Height);
        io.DisplayFramebufferScale = new System.Numerics.Vector2(1, 1);
        
        ImGui.GetStyle().ScaleAllSizes(1.5f);
        
        ImGuiImplOpenGL3.SetCurrentContext(context);
        
        if (!ImGuiImplOpenGL3.Init("#version 150"))
        {
            throw new Exception("Failed to initialize ImGui OpenGL3 backend");
        }
        
        ImGui.StyleColorsDark();
        
        // The above was the easy part, now we need to implement the mouse and keyboard input
        // This is done by forwarding the events to ImGui

        window.MouseButton += (buttonEvent) =>
        {
            var index = 0; // left mouse button
            if (buttonEvent.Button == (int)SDLMouseButtonFlags.Right)
            {
                index = 1; // right mouse button
            }
            else if (buttonEvent.Button == (int)SDLMouseButtonFlags.Middle)
            {
                index = 2; // middle mouse button
            }
            else if (buttonEvent.Button == (int)SDLMouseButtonFlags.X1)
            {
                index = 3; // extra mouse button 1
            }
            else if (buttonEvent.Button == (int)SDLMouseButtonFlags.X2)
            {
                index = 4; // extra mouse button 2
            }

            io.MouseDown[index] = buttonEvent.Type == SDLEventType.MouseButtonDown;
        };
        
        window.MouseWheel += (wheelEvent, x, y) =>
        {
            ImGui.GetIO().AddMouseWheelEvent(x, y);
        };
        
        window.MouseMove += (motionEvent, x, y) =>
        {
            ImGui.GetIO().AddMousePosEvent(x, y);
        };
        
        window.KeyDown += (keyDownEvent) =>
        {
            io.AddKeyEvent(TranslateKey(keyDownEvent.Scancode), true);
            io.AddInputCharacter(ScanCodeToChar(keyDownEvent));

            if (keyDownEvent.Mod != SDLKeymod.None)
            {
                io.KeyCtrl = (keyDownEvent.Mod & SDLKeymod.Ctrl) != 0;
                io.KeyShift = (keyDownEvent.Mod & SDLKeymod.Shift) != 0;
                io.KeyAlt = (keyDownEvent.Mod & SDLKeymod.Alt) != 0;
                io.KeySuper = (keyDownEvent.Mod & SDLKeymod.Gui) != 0;
            }
        };
        
        window.KeyUp += (keyUpEvent) =>
        {
            io.AddKeyEvent(TranslateKey(keyUpEvent.Scancode), false);
            
            if (keyUpEvent.Mod != SDLKeymod.None)
            {
                io.KeyCtrl = (keyUpEvent.Mod & SDLKeymod.Ctrl) != 0;
                io.KeyShift = (keyUpEvent.Mod & SDLKeymod.Shift) != 0;
                io.KeyAlt = (keyUpEvent.Mod & SDLKeymod.Alt) != 0;
                io.KeySuper = (keyUpEvent.Mod & SDLKeymod.Gui) != 0;
            }
        };
    }
    
    public void NewFrame()
    {
        ImGuiImplOpenGL3.NewFrame();
        ImGui.NewFrame();
    }
    
    public void Render()
    {
        ImGui.Render();
        ImGuiImplOpenGL3.RenderDrawData(ImGui.GetDrawData());
    }
    
    public void Shutdown()
    {
        ImGuiImplOpenGL3.Shutdown();
    }

    public ImGuiKey TranslateKey(SDLScancode scanCode)
    {
        return scanCode switch
        {
            SDLScancode.A => ImGuiKey.A,
            SDLScancode.B => ImGuiKey.B,
            SDLScancode.C => ImGuiKey.C,
            SDLScancode.D => ImGuiKey.D,
            SDLScancode.E => ImGuiKey.E,
            SDLScancode.F => ImGuiKey.F,
            SDLScancode.G => ImGuiKey.G,
            SDLScancode.H => ImGuiKey.H,
            SDLScancode.I => ImGuiKey.I,
            SDLScancode.J => ImGuiKey.J,
            SDLScancode.K => ImGuiKey.K,
            SDLScancode.L => ImGuiKey.L,
            SDLScancode.M => ImGuiKey.M,
            SDLScancode.N => ImGuiKey.N,
            SDLScancode.O => ImGuiKey.O,
            SDLScancode.P => ImGuiKey.P,
            SDLScancode.Q => ImGuiKey.Q,
            SDLScancode.R => ImGuiKey.R,
            SDLScancode.S => ImGuiKey.S,
            SDLScancode.T => ImGuiKey.T,
            SDLScancode.U => ImGuiKey.U,
            SDLScancode.V => ImGuiKey.V,
            SDLScancode.W => ImGuiKey.W,
            SDLScancode.X => ImGuiKey.X,
            SDLScancode.Y => ImGuiKey.Y,
            SDLScancode.Z => ImGuiKey.Z,
            SDLScancode.Scancode0 => ImGuiKey.Key0,
            SDLScancode.Scancode1 => ImGuiKey.Key1,
            SDLScancode.Scancode2 => ImGuiKey.Key2,
            SDLScancode.Scancode3 => ImGuiKey.Key3,
            SDLScancode.Scancode4 => ImGuiKey.Key4,
            SDLScancode.Scancode5 => ImGuiKey.Key5,
            SDLScancode.Scancode6 => ImGuiKey.Key6,
            SDLScancode.Scancode7 => ImGuiKey.Key7,
            SDLScancode.Scancode8 => ImGuiKey.Key8,
            SDLScancode.Scancode9 => ImGuiKey.Key9,
            SDLScancode.Return => ImGuiKey.Enter,
            SDLScancode.Escape => ImGuiKey.Escape,
            SDLScancode.Backspace => ImGuiKey.Backspace,
            SDLScancode.Tab => ImGuiKey.Tab,
            SDLScancode.Space => ImGuiKey.Space,
            SDLScancode.Minus => ImGuiKey.Minus,
            SDLScancode.Equals => ImGuiKey.Equal,
            SDLScancode.Leftbracket => ImGuiKey.LeftBracket,
            SDLScancode.Rightbracket => ImGuiKey.RightBracket,
            SDLScancode.Backslash => ImGuiKey.Backslash,
            SDLScancode.Kp0 => ImGuiKey.Keypad0,
            SDLScancode.Kp1 => ImGuiKey.Keypad1,
            SDLScancode.Kp2 => ImGuiKey.Keypad2,
            SDLScancode.Kp3 => ImGuiKey.Keypad3,
            SDLScancode.Kp4 => ImGuiKey.Keypad4,
            SDLScancode.Kp5 => ImGuiKey.Keypad5,
            SDLScancode.Kp6 => ImGuiKey.Keypad6,
            SDLScancode.Kp7 => ImGuiKey.Keypad7,
            SDLScancode.Kp8 => ImGuiKey.Keypad8,
            SDLScancode.Kp9 => ImGuiKey.Keypad9,
            SDLScancode.KpPeriod => ImGuiKey.KeypadDecimal,
            SDLScancode.KpDivide => ImGuiKey.KeypadDivide,
            SDLScancode.KpMultiply => ImGuiKey.KeypadMultiply,
            SDLScancode.KpMinus => ImGuiKey.KeypadSubtract,
            SDLScancode.KpPlus => ImGuiKey.KeypadAdd,
            SDLScancode.KpEnter => ImGuiKey.KeypadEnter,
            SDLScancode.KpEquals => ImGuiKey.KeypadEqual,
            SDLScancode.Up => ImGuiKey.UpArrow,
            SDLScancode.Down => ImGuiKey.DownArrow,
            SDLScancode.Right => ImGuiKey.RightArrow,
            SDLScancode.Left => ImGuiKey.LeftArrow,
            SDLScancode.Insert => ImGuiKey.Insert,
            SDLScancode.Home => ImGuiKey.Home,
            SDLScancode.End => ImGuiKey.End,
            SDLScancode.Pageup => ImGuiKey.PageUp,
            SDLScancode.Pagedown => ImGuiKey.PageDown,
            SDLScancode.F1 => ImGuiKey.F1,
            SDLScancode.F2 => ImGuiKey.F2,
            SDLScancode.F3 => ImGuiKey.F3,
            SDLScancode.F4 => ImGuiKey.F4,
            SDLScancode.F5 => ImGuiKey.F5,
            SDLScancode.F6 => ImGuiKey.F6,
            SDLScancode.F7 => ImGuiKey.F7,
            SDLScancode.F8 => ImGuiKey.F8,
            SDLScancode.F9 => ImGuiKey.F9,
            SDLScancode.F10 => ImGuiKey.F10,
            SDLScancode.F11 => ImGuiKey.F11,
            SDLScancode.F12 => ImGuiKey.F12,
            SDLScancode.F13 => ImGuiKey.F13,
            SDLScancode.F14 => ImGuiKey.F14,
            SDLScancode.F15 => ImGuiKey.F15,
            SDLScancode.Numlockclear => ImGuiKey.NumLock,
            SDLScancode.Capslock => ImGuiKey.CapsLock,
            SDLScancode.Scrolllock => ImGuiKey.ScrollLock,
            SDLScancode.Rshift => ImGuiKey.RightShift,
            SDLScancode.Lshift => ImGuiKey.LeftShift,
            SDLScancode.Rctrl => ImGuiKey.RightCtrl,
            SDLScancode.Lctrl => ImGuiKey.LeftCtrl,
            SDLScancode.Ralt => ImGuiKey.RightAlt,
            SDLScancode.Lalt => ImGuiKey.LeftAlt,
            SDLScancode.Rgui => ImGuiKey.RightSuper,
            SDLScancode.Lgui => ImGuiKey.LeftSuper,
            SDLScancode.Application => ImGuiKey.Menu,
            _ => ImGuiKey.None
        };
    }
    
    public char ScanCodeToChar(SDLKeyboardEvent keyEvent)
    {
        var @char = RawScanCodeToChar(keyEvent.Scancode);

        if (keyEvent.Mod != SDLKeymod.None)
        {
            bool pressedShift = (keyEvent.Mod & SDLKeymod.Shift) != 0;

            if (pressedShift)
            {
                if (@char >= 'a' && @char <= 'z')
                {
                    @char = char.ToUpper(@char);
                }
                else
                {
                    switch (@char)
                    {
                        case '1': @char = '!'; break;
                        case '2': @char = '@'; break;
                        case '3': @char = '#'; break;
                        case '4': @char = '$'; break;
                        case '5': @char = '%'; break;
                        case '6': @char = '^'; break;
                        case '7': @char = '&'; break;
                        case '8': @char = '*'; break;
                        case '9': @char = '('; break;
                        case '0': @char = ')'; break;
                        case '-': @char = '_'; break;
                        case '=': @char = '+'; break;
                        case '[': @char = '{'; break;
                        case ']': @char = '}'; break;
                        case '\\': @char = '|'; break;
                        case ';': @char = ':'; break;
                        case '\'': @char = '"'; break;
                        case ',': @char = '<'; break;
                        case '.': @char = '>'; break;
                        case '/': @char = '?'; break;
                    }
                }
            }
        }
        
        return @char;
    }
    
    private char RawScanCodeToChar(SDLScancode objScancode)
    {
        switch (objScancode)
        {
            case SDLScancode.A: return 'a';
            case SDLScancode.B: return 'b';
            case SDLScancode.C: return 'c';
            case SDLScancode.D: return 'd';
            case SDLScancode.E: return 'e';
            case SDLScancode.F: return 'f';
            case SDLScancode.G: return 'g';
            case SDLScancode.H: return 'h';
            case SDLScancode.I: return 'i';
            case SDLScancode.J: return 'j';
            case SDLScancode.K: return 'k';
            case SDLScancode.L: return 'l';
            case SDLScancode.M: return 'm';
            case SDLScancode.N: return 'n';
            case SDLScancode.O: return 'o';
            case SDLScancode.P: return 'p';
            case SDLScancode.Q: return 'q';
            case SDLScancode.R: return 'r';
            case SDLScancode.S: return 's';
            case SDLScancode.T: return 't';
            case SDLScancode.U: return 'u';
            case SDLScancode.V: return 'v';
            case SDLScancode.W: return 'w';
            case SDLScancode.X: return 'x';
            case SDLScancode.Y: return 'y';
            case SDLScancode.Z: return 'z';
            case SDLScancode.Space: return ' ';
            case SDLScancode.Return: return '\n';
            case SDLScancode.Backspace: return '\b';
            case SDLScancode.Tab: return '\t';
            case SDLScancode.Minus: return '-';
            case SDLScancode.Equals: return '=';
            case SDLScancode.Leftbracket: return '[';
            case SDLScancode.Rightbracket: return ']';
            case SDLScancode.Backslash: return '\\';
            case SDLScancode.Semicolon: return ';';
            case SDLScancode.Apostrophe: return '\'';
            case SDLScancode.Grave: return '`';
            case SDLScancode.Comma: return ',';
            case SDLScancode.Period: return '.';
            case SDLScancode.Slash: return '/';
            case SDLScancode.Kp0: return '0';
            case SDLScancode.Kp1: return '1';
            case SDLScancode.Kp2: return '2';
            case SDLScancode.Kp3: return '3';
            case SDLScancode.Kp4: return '4';
            case SDLScancode.Kp5: return '5';
            case SDLScancode.Kp6: return '6';
            case SDLScancode.Kp7: return '7';
            case SDLScancode.Kp8: return '8';
            case SDLScancode.Kp9: return '9';
            case SDLScancode.Scancode0: return '0';
            case SDLScancode.Scancode1: return '1';
            case SDLScancode.Scancode2: return '2';
            case SDLScancode.Scancode3: return '3';
            case SDLScancode.Scancode4: return '4';
            case SDLScancode.Scancode5: return '5';
            case SDLScancode.Scancode6: return '6';
            case SDLScancode.Scancode7: return '7';
            case SDLScancode.Scancode8: return '8';
            case SDLScancode.Scancode9: return '9';
            case SDLScancode.KpPeriod: return '.';
            case SDLScancode.KpDivide: return '/';
            case SDLScancode.KpMultiply: return '*';
            case SDLScancode.KpMinus: return '-';
            case SDLScancode.KpPlus: return '+';
            case SDLScancode.KpEnter: return '\n';
            case SDLScancode.KpEquals: return '=';
            default: return '\0';
        }
    }

    public void SetResolution(int width, int height)
    {
        ImGui.GetIO().DisplaySize = new System.Numerics.Vector2(width, height);
        ImGui.GetIO().DisplayFramebufferScale = new System.Numerics.Vector2(1, 1);
    }
}