using System.Runtime.InteropServices;

namespace System03.Windowing.Native.Win32;

internal sealed class Win32Bindings
{
    private const string LibraryName = "user32.dll";

    [DllImport(LibraryName, EntryPoint = "CreateWindowEx")]
    public static extern IntPtr CreateWindowEx(
        uint dwExStyle,
        string? lpClassName,
        string? lpWindowName,
        uint dwStyle,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr? hWndParent,
        IntPtr? hMenu,
        IntPtr? hInstance,
        IntPtr? lpParam
    );
    
    [DllImport(LibraryName, EntryPoint = "DestroyWindow")]
    public static extern bool DestroyWindow(IntPtr hWnd);
}