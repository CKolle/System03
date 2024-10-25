using System03.Windowing.Native.X11;

namespace System03.Windowing.Platform;

// Don't think this will work. But I will leave it here for nwo
internal interface IX11EventHandler
{
    /// <summary>
    /// Says that the event handler should poll the next event.
    /// </summary>
    /// <param name="display"></param>
     void HandleEvent(IntPtr display, X11Bindings.XEvent xEvent);
}