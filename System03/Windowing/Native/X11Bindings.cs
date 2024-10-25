using System.Runtime.InteropServices;

namespace System03.Windowing.Native.X11;

internal static class X11Bindings
{
    private const string LibraryName = "libX11.so.6";

    // Define the X event types
    public enum XEventName
    {
        KeyPress = 2,
        KeyRelease = 3,
        ButtonPress = 4,
        ButtonRelease = 5,
        MotionNotify = 6,
        EnterNotify = 7,
        LeaveNotify = 8,
        FocusIn = 9,
        FocusOut = 10,
        KeymapNotify = 11,
        Expose = 12,
        GraphicsExpose = 13,
        NoExpose = 14,
        VisibilityNotify = 15,
        CreateNotify = 16,
        DestroyNotify = 17,
        UnmapNotify = 18,
        MapNotify = 19,
        MapRequest = 20,
        ReparentNotify = 21,
        ConfigureNotify = 22,
        ConfigureRequest = 23,
        GravityNotify = 24,
        ResizeRequest = 25,
        CirculateNotify = 26,
        CirculateRequest = 27,
        PropertyNotify = 28,
        SelectionClear = 29,
        SelectionRequest = 30,
        SelectionNotify = 31,
        ColormapNotify = 32,
        ClientMessage = 33,
        MappingNotify = 34,
        GenericEvent = 35,
        LASTEvent
    }

    public enum ColorMapAlloc
    {
        None = 0,
        All = 1
    }
    [StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
    public struct XAnyEvent
    {
        public XEventName type;
        public nint serial;      // Changed from IntPtr
        public bool send_event;
        public nint display;     // Changed from IntPtr
        public nint window;      // Changed from IntPtr
    }
    
    [StructLayout(LayoutKind.Sequential, Size = (24 * sizeof(long)))]
    public struct XKeyEvent
    {
        public XEventName type;
        public nint serial;      // Changed from IntPtr
        public bool send_event;
        public nint display;     // Changed from IntPtr
        public nint window;      // Changed from IntPtr
        public nint root;
        public nint subwindow;
        public nint time;
        public int x, y;
        public int x_root, y_root;
        public uint state;
        public uint keycode;
        public bool same_screen;
    }
    
    // Define the XEvent structure to handle events from the X server
    // A XEvent is technically a union but we only use XKeyEvent
    // Also since we don't have a size the allocation will crash the program
    // TODO - This is not entirely correct but it works for now
    [StructLayout(LayoutKind.Sequential)]
    public struct XEvent
    {
        [MarshalAs(UnmanagedType.I4)]
        public XEventName type;
        public nint serial;    
        [MarshalAs(UnmanagedType.I1)]
        public bool send_event;
        public nint display;    
        public nint window;     
        public int root;
        public int subwindow;
        public int time;
        public int x, y;
        public int x_root, y_root;
        public uint state;
        public uint keycode;
        [MarshalAs(UnmanagedType.I1)]
        public bool same_screen;
    }

    // Struct for XWindowAttributes
    [StructLayout(LayoutKind.Sequential)]
    public struct XWindowAttributes
    {
        public int x, y; // location of window
        public int width, height; // size of window
        public int border_width; // border width
        public int depth; // depth of window
        public IntPtr visual; // the visual type
        public IntPtr root; // the root window
        public int class_hint; // input/output class
        public int bit_gravity, win_gravity; // window gravity
        public int backing_store; // NotUseful, WhenMapped, Always
        public ulong backing_planes; // planes to be preserved if possible
        public ulong backing_pixel; // value to use in restoring planes
        public bool save_under; // should bits under be saved?
        public IntPtr colormap; // color map to be installed
        public bool map_installed; // boolean, is color map currently installed?
        public int map_state; // IsUnmapped, IsUnviewable, IsViewable
        public long all_event_masks; // all events that can be selected
        public long your_event_mask; // events selected by this client
        public long do_not_propagate_mask; // do not propagate these events
        public bool override_redirect; // boolean, should override redirect?
        public IntPtr screen; // the screen the window is on
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XSetWindowAttributes
    {
        public IntPtr background_pixmap;
        public long background_pixel;
        public IntPtr border_pixmap;
        public long border_pixel;
        public int bit_gravity;
        public int win_gravity;
        public int backing_store;
        public long backing_planes;
        public long backing_pixel;
        public bool save_under;
        public EventMask event_mask;
        public EventMask do_not_propagate_mask;
        public bool override_redirect;
        public IntPtr colormap;
        public IntPtr cursor;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XVisualInfo
    {
        public IntPtr visual;
        public IntPtr visualid;
        public int screen;
        public int depth;
        public int @class;
        public ulong red_mask;
        public ulong green_mask;
        public ulong blue_mask;
        public int colormap_size;
        public int bits_per_rgb;
    }

    // Add the true color visual class
    public const int TrueColor = 4;

    // InputOutput class
    public const uint InputOutput = 1;

    // Open a connection to the X server
    [DllImport(LibraryName, EntryPoint = "XOpenDisplay")]
    public static extern IntPtr OpenDisplay(string? displayName);

    // Create a simple window on the display
    [DllImport(LibraryName, EntryPoint = "XCreateSimpleWindow")]
    public static extern IntPtr CreateSimpleWindow(IntPtr display, IntPtr parent, int x, int y, uint width, uint height,
        uint borderWidth, ulong border, ulong background);

    // Get the root window of the display
    [DllImport(LibraryName, EntryPoint = "XDefaultRootWindow")]
    public static extern IntPtr DefaultRootWindow(IntPtr display);

    [DllImport(LibraryName, EntryPoint = "XLookupKeysym")]
    public static extern uint LookupKeysym(ref XKeyEvent keyEvent, int index);

    [DllImport(LibraryName, EntryPoint = "XRootWindow")]
    public static extern IntPtr RootWindow(IntPtr display, int screenNumber);

    // Map a window to make it visible on the screen
    [DllImport(LibraryName, EntryPoint = "XMapWindow")]
    public static extern void MapWindow(IntPtr display, IntPtr window);

    // Check how many events are pending
    [DllImport(LibraryName, EntryPoint = "XPending")]
    public static extern int Pending(IntPtr display);

    // Get the next event from the event queue
    [DllImport(LibraryName, EntryPoint = "XNextEvent")]
    public static extern void NextEvent(IntPtr display, IntPtr eventReturn);

    // Select input events for a window
    [DllImport(LibraryName, EntryPoint = "XSelectInput")]
    public static extern void SelectInput(IntPtr display, IntPtr window, EventMask eventMask);

    // Get window attributes
    [DllImport(LibraryName, EntryPoint = "XGetWindowAttributes")]
    public static extern int GetWindowAttributes(IntPtr display, IntPtr window, ref XWindowAttributes attributes);

    // Close the connection to the X server
    [DllImport(LibraryName, EntryPoint = "XCloseDisplay")]
    public static extern int CloseDisplay(IntPtr display);
    
    [DllImport(LibraryName, EntryPoint = "XDefaultScreen")]
    public static extern int DefaultScreen(IntPtr display);
    
    [DllImport(LibraryName, EntryPoint = "XGetVisualInfo")]
    public static extern IntPtr GetVisualInfo(IntPtr display, int vinfo_mask, ref XVisualInfo vinfo_template, out int nitems);

    // Flush the output buffer, ensuring all requests are sent
    [DllImport(LibraryName, EntryPoint = "XFlush")]
    public static extern void Flush(IntPtr display);

    // Destroy a window
    [DllImport(LibraryName, EntryPoint = "XDestroyWindow")]
    public static extern void DestroyWindow(IntPtr display, IntPtr window);

    // Send an event to a window
    [DllImport(LibraryName, EntryPoint = "XSendEvent")]
    public static extern int SendEvent(IntPtr display, IntPtr window, bool propagate, long event_mask, ref XEvent send_event);

    // Move a window to a new position
    [DllImport(LibraryName, EntryPoint = "XMoveWindow")]
    public static extern void MoveWindow(IntPtr display, IntPtr window, int x, int y);
    
    [DllImport(LibraryName, EntryPoint = "XCreateColormap")]
    public static extern IntPtr CreateColormap(IntPtr display, IntPtr window, IntPtr visual, ColorMapAlloc alloc);
    
    [DllImport(LibraryName, EntryPoint = "XCreateWindow")]
    public static extern IntPtr CreateWindow(IntPtr display, IntPtr parent, int x, int y, uint width, uint height, uint border_width, int depth, uint @class, IntPtr visual, ulong valuemask, ref XSetWindowAttributes attributes);
    
    [Flags]
    public enum VisualMask
    {
        VisualScreenMask = 1,
        VisualDepthMask = 2,
        VisualClassMask = 4
    }

    [Flags]
    public enum EventMask : long
    {
        NoEventMask = 0L,
        KeyPressMask = 1L << 0,
        KeyReleaseMask = 1L << 1,
        ButtonPressMask = 1L << 2,
        ButtonReleaseMask = 1L << 3,
        EnterWindowMask = 1L << 4,
        LeaveWindowMask = 1L << 5,
        PointerMotionMask = 1L << 6,
        PointerMotionHintMask = 1L << 7,
        Button1MotionMask = 1L << 8,
        Button2MotionMask = 1L << 9,
        Button3MotionMask = 1L << 10,
        Button4MotionMask = 1L << 11,
        Button5MotionMask = 1L << 12,
        ButtonMotionMask = 1L << 13,
        KeymapStateMask = 1L << 14,
        ExposureMask = 1L << 15,
        VisibilityChangeMask = 1L << 16,
        StructureNotifyMask = 1L << 17,
        ResizeRedirectMask = 1L << 18,
        SubstructureNotifyMask = 1L << 19,
        SubstructureRedirectMask = 1L << 20,
        FocusChangeMask = 1L << 21,
        PropertyChangeMask = 1L << 22,
        ColormapChangeMask = 1L << 23,
        OwnerGrabButtonMask = 1L << 24
    }
    
    [Flags]
    public enum WindowAttributeMask : long
    {
        CWBackPixmap = 1L << 0,
        CWBackPixel = 1L << 1,
        CWBorderPixmap = 1L << 2,
        CWBorderPixel = 1L << 3,
        CWBitGravity = 1L << 4,
        CWWinGravity = 1L << 5,
        CWBackingStore = 1L << 6,
        CWBackingPlanes = 1L << 7,
        CWBackingPixel = 1L << 8,
        CWOverrideRedirect = 1L << 9,
        CWSaveUnder = 1L << 10,
        CWEventMask = 1L << 11,
        CWDontPropagate = 1L << 12,
        CWColormap = 1L << 13,
        CWCursor = 1L << 14
    }
}
