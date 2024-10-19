using System.Runtime.InteropServices;
using System03.Windowing.Abstractions;
using System03.Windowing.Native.X11;

namespace System03.Windowing.Platform
{
    public sealed class X11WindowSystem : IWindowSystem
    {
        private IntPtr _display;
        private IntPtr _window;
        private X11Bindings.XVisualInfo _visualInfo;
        private IntPtr _visual;
        private int _screen;
        private int _height;
        private bool _disposed;
        
        public X11WindowSystem(int width, int height, bool isEmbedded)
        {
            _display = X11Bindings.OpenDisplay(null);
            if (_display == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to open display");
            }

            _screen = X11Bindings.DefaultScreen(_display);
            IntPtr rootWindow = X11Bindings.RootWindow(_display, _screen);

            // Setup visual info template
            var visualTemplate = new X11Bindings.XVisualInfo
            {
                screen = _screen,
                depth = 24,
                @class = X11Bindings.TrueColor
            };

            int visualCount;
            IntPtr visualList = X11Bindings.GetVisualInfo(_display, 0, ref visualTemplate, out visualCount);

            if (visualList == IntPtr.Zero || visualCount == 0)
            {
                throw new InvalidOperationException("Failed to get visual info");
            }

            _visualInfo = Marshal.PtrToStructure<X11Bindings.XVisualInfo>(visualList);
            _visual = _visualInfo.visual;

            IntPtr colormap = X11Bindings.CreateColormap(_display, rootWindow, _visual, X11Bindings.ColorMapAlloc.None);

            var attributes = new X11Bindings.XSetWindowAttributes
            {
                colormap = colormap,
                backing_pixel = 0,
                border_pixel = 0,
                event_mask = X11Bindings.EventMask.StructureNotifyMask |
                             X11Bindings.EventMask.ExposureMask |
                             X11Bindings.EventMask.KeyPressMask |
                             X11Bindings.EventMask.KeyReleaseMask |
                             X11Bindings.EventMask.ButtonPressMask |
                             X11Bindings.EventMask.ButtonReleaseMask |
                             X11Bindings.EventMask.PointerMotionMask
            };

            _window = X11Bindings.CreateWindow(
                _display,
                rootWindow,
                0, 0,
                (uint)width, (uint)height,
                0,                      // Border width
                _visualInfo.depth,       // Depth
                X11Bindings.InputOutput, // Class
                _visual,                 // Visual
                (ulong)(X11Bindings.WindowAttributeMask.CWColormap |
                        X11Bindings.WindowAttributeMask.CWEventMask |
                        X11Bindings.WindowAttributeMask.CWBackPixel |
                        X11Bindings.WindowAttributeMask.CWBorderPixel),
                ref attributes
            );

            if (_window == IntPtr.Zero)
            {
                throw new InvalidOperationException("Failed to create window");
            }
            
            if (!isEmbedded)
            {
                X11Bindings.MapWindow(_display, _window);
            }

            X11Bindings.Flush(_display);
            
        }

        IntPtr IWindowSystem.NativeHandle()
        {
            return _window;
        }

        IntPtr IWindowSystem.NativeDisplay()
        {
            return _display;
        }
        
        public bool ShouldClose()
        {
            if (_display == IntPtr.Zero || _window == IntPtr.Zero)
            {
                throw new InvalidOperationException("Window system is not initialized");
            }
            
            while (X11Bindings.Pending(_display) > 0)
            {
                X11Bindings.XEvent xEvent = default;
                X11Bindings.NextEvent(_display, ref xEvent);
                if (xEvent.type == X11Bindings.XEventName.KeyPress)
                {
                    return true;
                }
            }

            return false;
        }
        

        public void Shutdown()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_disposed) return;

            if (_window != IntPtr.Zero)
            {
                X11Bindings.DestroyWindow(_display, _window);
                _window = IntPtr.Zero;
            }

            if (_display != IntPtr.Zero)
            {
                X11Bindings.CloseDisplay(_display);
                _display = IntPtr.Zero;
            }

            _disposed = true;
        }

        public IntPtr NativeDisplay() => _display;

        ~X11WindowSystem()
        {
            Dispose();
        }
    }
}
