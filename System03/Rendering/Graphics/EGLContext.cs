using System03.Windowing.Abstractions;
using System03.Windowing.Native.EGL;

namespace System03.Rendering.Graphics;

public class EGLContext : IGraphicsContext
{
    private IntPtr _eglDisplay;
    private IntPtr _eglSurface;
    private IntPtr _eglContext;
    private IWindowSystem _window;

    public EGLContext(IWindowSystem window)
    {
        _window = window;
    }
    public void Initialize()
    {
        _eglDisplay = EGLBindings.GetDisplay(_window.NativeDisplay());
        
        if (_eglDisplay == IntPtr.Zero)
        {
            int error = EGLBindings.GetError();
            throw new InvalidOperationException($"Failed to get EGL display: {error}");
        }
        
        if (!EGLBindings.Initialize(_eglDisplay, out var major, out var minor))
        {
            int error = EGLBindings.GetError();
            throw new InvalidOperationException($"Failed to initialize X11 display: {error}");
        }
        
        int[] configAttribs = {
            0x3024, 8,        // EGL_RED_SIZE
            0x3023, 8,        // EGL_GREEN_SIZE
            0x3022, 8,        // EGL_BLUE_SIZE
            0x3021, 8,        // EGL_ALPHA_SIZE
            0x3025, 24,       // EGL_DEPTH_SIZE
            0x3026, 8,        // EGL_STENCIL_SIZE
            0x3033, 1,        // EGL_NATIVE_RENDERABLE
            0x3038, 0x0004,   // EGL_SURFACE_TYPE = EGL_WINDOW_BIT
            0x3027, 1,        // EGL_RENDERABLE_TYPE = EGL_OPENGL_BIT
            0x3038            // EGL_NONE
        };
        
        IntPtr[] configs = new IntPtr[1];
        if (!EGLBindings.ChooseConfig(_eglDisplay, configAttribs, configs, configs.Length, out var numConfigs) || numConfigs == 0)
        {
            int error = EGLBindings.GetError();
            throw new InvalidOperationException($"Failed to choose EGL config. Error: 0x{error:X}");
        }
        if (numConfigs != 1)
        {
            throw new InvalidOperationException($"Expected 1 EGL config, but got {numConfigs}");
        }
        
        IntPtr eglConfig = configs[0];
        
        _eglSurface = EGLBindings.CreateWindowSurface(_eglDisplay, eglConfig, _window.NativeHandle(), null);
        if (_eglSurface == IntPtr.Zero)
        {
            int error = EGLBindings.GetError();
            throw new InvalidOperationException($"Failed to create X11 window surface. Error: 0x{error:X}");
        }
        
        // Create OpenGL context. We use ES 2.0 so it translates nicely to WebGL
        // Well scrap that, we are using OpenGL ES 3.0, just remembered WebGL 2 exists
        int[] contextAttribs = { 0x3098, 3, 0x3038, 0 }; // EGL_CONTEXT_CLIENT_VERSION, 2 for OpenGL ES 2

        _eglContext = EGLBindings.CreateContext(_eglDisplay, eglConfig, IntPtr.Zero, contextAttribs);
        if (_eglContext == IntPtr.Zero)
        {
            int error = EGLBindings.GetError();
            throw new InvalidOperationException($"Failed to create EGL context. Error: 0x{error:X}");
        }

    }

    public void Dispose()
    {
        // TODO: Add bindings to destroy the context and surface
        
    }

    public void MakeCurrent()
    {
        EGLBindings.MakeCurrent(_eglDisplay, _eglSurface, _eglSurface, _eglContext);
    }

    public void SwapBuffers()
    {
        EGLBindings.SwapInterval(_eglDisplay, 0);
        EGLBindings.SwapBuffers(_eglDisplay, _eglSurface);
    }

    public IntPtr GetProcAddress(string procName)
    {
        return EGLBindings.GetProcAddress(procName);
    }
}