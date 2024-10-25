using System.Diagnostics;
using System03.Input;
using System03.Windowing.Native.X11;

namespace System03.Windowing.Platform;

public sealed class X11Keyboard : IKeyboardInputDevice
{
    private IntPtr _display;
    // True if the key is down. So logical high means the key is pressed.
    private readonly Dictionary<Key, bool> _keyStates;
    internal X11Keyboard(IntPtr display)
    {
        _display = display;
        
        _keyStates = new Dictionary<Key, bool>();
        
        // Initialize all key states to false
        foreach (Key key in Enum.GetValues<Key>())
        {
            _keyStates[key] = false;
        }
    }

    /* This is stupid. Why can't I implement an internal method for an interface if they are in the same assembly?
    internal void HandleEvent(IntPtr display, X11Bindings.XEvent xEvent)
    {
        throw new NotImplementedException();
    }
    */
    
    public bool IsKeyDown(Key key)
    {
        return _keyStates.TryGetValue(key, out var state) && state;
    }

    public bool IsKeyUp(Key key)
    {
        return !IsKeyDown(key);
    }
    
    internal void HandleKeyPress(X11Bindings.XKeyEvent keyEvent)
    {
        var keysym = X11Bindings.LookupKeysym(ref keyEvent, 0);
        var key = ConvertKeyCodeToKey(keysym);
        Console.WriteLine($"Key pressed: {key}");
        _keyStates[key] = true;
    }
    
    internal void HandleKeyRelease(X11Bindings.XKeyEvent keyEvent)
    {
        var keysym = X11Bindings.LookupKeysym(ref keyEvent, 0);
        var key = ConvertKeyCodeToKey(keysym);
        Console.WriteLine($"Key released: {key}");
        _keyStates[key] = false;
    }
    
    private Key ConvertKeyCodeToKey(uint keyCode)
    {
        return keyCode switch
        {
            113 => Key.Q,
            119 => Key.W,
            101 => Key.E,
            114 => Key.R,
            116 => Key.T,
            121 => Key.Y,
            117 => Key.U,
            105 => Key.I,
            111 => Key.O,
            112 => Key.P,
            97 => Key.A,
            115 => Key.S,
            100 => Key.D,
            102 => Key.F,
            103 => Key.G,
            104 => Key.H,
            106 => Key.J,
            107 => Key.K,
            108 => Key.L,
            122 => Key.Z,
            120 => Key.X,
            99 => Key.C,
            118 => Key.V,
            98 => Key.B,
            110 => Key.N,
            109 => Key.M,
            _ => Key.Unknown
        };
    }
}