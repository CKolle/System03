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

    public WindowBuilder AsEmbedded()
    {
        _config = _config with { IsEmbedded = true };
        return this;
    }

    public IWindowSystem Build()
    {
        _backend = _backend == WindowBackend.Auto ? DetermineBackend() : _backend;

        return _backend switch
        {
            WindowBackend.X11 => new X11WindowSystem(_config.Width, _config.Height, _config.IsEmbedded),
            WindowBackend.Wayland => throw new NotImplementedException(),
            _ => throw new PlatformNotSupportedException()
        };
    }


    private static WindowBackend DetermineBackend()
    {
        // If contains wayland in the environment variables, use Wayland
        if (Environment.GetEnvironmentVariable("WAYLAND_DISPLAY") != null)
        {
            return WindowBackend.Wayland;
        }
        // Defaults to X11
        return WindowBackend.X11;
    }
}