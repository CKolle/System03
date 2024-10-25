using System.Runtime.InteropServices;
using System03.Input;
using System03.Windowing.Native.X11;

namespace System03.Windowing.Platform;

public sealed class X11PollGroup : IPollGroup
{
    private volatile IntPtr _display;
    private X11Keyboard _keyboard;

    public X11PollGroup(IntPtr display, X11Keyboard keyboard)
    {
        _display = display;
        _keyboard = keyboard;
    }

    public void PollGroup()
    {
        while (X11Bindings.Pending(_display) != 0)
        {
            // Just allocate as key event as XEvent is a union and we only care about key events
            IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf<X11Bindings.XKeyEvent>());
            X11Bindings.NextEvent(_display, ptr);
            // Check if the pointer is null
            var xEvent = Marshal.PtrToStructure<X11Bindings.XKeyEvent>(ptr);
            switch (xEvent.type)
            {
                case X11Bindings.XEventName.KeyPress:
                    _keyboard.HandleKeyPress(xEvent);
                    break;
                case X11Bindings.XEventName.KeyRelease:
                    _keyboard.HandleKeyRelease(xEvent);
                    break;
            }
            Marshal.FreeHGlobal(ptr);
        }
        
    }
}