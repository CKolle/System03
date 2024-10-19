using System;
using Avalonia.Controls;
using Avalonia.Platform;
using System03.Rendering.Graphics;
using System03.Vinduer;
using System03.Windowing.Abstractions;

namespace System03.Editor.Controls;

public class OpenGLSurface : NativeControlHost
{
    private IWindowSystem _windowSystem;
    private IGraphicsContext _context;
    public new uint Width { get; private set; }
    public new uint Height { get; private set; }


    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        Width = (uint)e.NewSize.Width;
        Height = (uint)e.NewSize.Height;
    }
    

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle control)
    {
        Console.WriteLine($"Window handle: 0x{control.Handle.ToInt64():X}");
        Console.WriteLine($"Handle descriptor: {control.HandleDescriptor}");
        
        _windowSystem = new WindowBuilder()
            .WithBackend(WindowBackend.X11)
            .AsEmbedded()
            .Build();
        
        var handle = _windowSystem.NativeHandle();
        
        _context = new EGLContext();
        _context.Initialize(_windowSystem);
        _context.MakeCurrent();
        
        return new PlatformHandle(handle, "X11");
    }
}