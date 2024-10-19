using System.Runtime.InteropServices;

namespace System03.Windowing.Native.EGL;

internal sealed class EGLBindings
{
    private const string LibraryName = "libEGL.so";
    
    public const int EGL_PLATFORM_WAYLAND_KHR = 0x31D8;
    
    [DllImport(LibraryName, EntryPoint = "eglGetDisplay")]
    public static extern IntPtr GetDisplay(IntPtr nativeDisplay);
    
    [DllImport(LibraryName, EntryPoint = "eglGetPlatformDisplay")]
    public static extern IntPtr GetPlatformDisplay(int platform, IntPtr nativeDisplay, int[] attribList);
    
    [DllImport(LibraryName, EntryPoint = "eglInitialize")]
    public static extern bool Initialize(IntPtr dpy, out int major, out int minor);
    
    [DllImport(LibraryName, EntryPoint = "eglChooseConfig")]
    public static extern bool ChooseConfig(IntPtr dpy, int[] attribList, IntPtr[] configs, int configSize, out int numConfig);
    
    [DllImport(LibraryName, EntryPoint = "eglCreateContext")]
    public static extern IntPtr CreateContext(IntPtr dpy, IntPtr config, IntPtr shareContext, int[] attribList);
    
    [DllImport(LibraryName, EntryPoint = "eglCreateWindowSurface")]
    public static extern IntPtr CreateWindowSurface(IntPtr dpy, IntPtr config, IntPtr win, int[] attribList);
    
    [DllImport(LibraryName, EntryPoint = "eglMakeCurrent")]
    public static extern bool MakeCurrent(IntPtr dpy, IntPtr draw, IntPtr read, IntPtr ctx);
    
    [DllImport(LibraryName, EntryPoint = "eglSwapBuffers")]
    public static extern bool SwapBuffers(IntPtr dpy, IntPtr surface);

    [DllImport(LibraryName, EntryPoint = "eglGetError")]
    public static extern int GetError();
    
    [DllImport(LibraryName, EntryPoint = "eglGetProcAddress")]
    public static extern IntPtr GetProcAddress(string procName);
}