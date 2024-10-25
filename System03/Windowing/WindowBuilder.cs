using System03.Windowing.Abstractions;
using System03.Windowing.Platform;

namespace System03.Vinduer;

public class WindowBuilder
{
    private WindowConfig _config = new();
    private WindowBackend _backend = WindowBackend.Auto;

    
    public WindowBuilder WithDimensions(int width, int height)
    {
        _config = _config with { Width = width, Height = height };
        return this;
    }

    public WindowBuilder WithBackend(WindowBackend backend)
    {
        _backend = backend;
        return this;
    }

    public WindowBuilder IsEmbedded(bool isEmbedded)
    {
        _config = _config with { IsEmbedded = isEmbedded };
        return this;
    }

    public IWindowSystem Build()
    {
        _backend = _backend == WindowBackend.Auto ? DetermineBackend() : _backend;

        return _backend switch
        {
            WindowBackend.X11 => new X11WindowSystem(_config.Width, _config.Height, _config.IsEmbedded),
            WindowBackend.Wayland => throw new NotImplementedException(),
            WindowBackend.Windows => new Win32WindowSystem(),
            _ => throw new PlatformNotSupportedException()
        };
    }


    private static WindowBackend DetermineBackend()
    {
        if (OperatingSystem.IsLinux())
        {
            // If Wayland is in the environment variables, use Wayland
            if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") != null)
            {
                // Wayland is not implemented yet, so default to X11
                return WindowBackend.X11;
            }
            // Default to X11 for Linux
            return WindowBackend.X11;
        }
        if (OperatingSystem.IsWindows())
        {
            return WindowBackend.Windows;
        }

        throw new PlatformNotSupportedException("Unsupported platform");
    }

}