using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform;
using System03.Core;
using System03.Core.Scene;


namespace System03.Editor.Controls;

public class OpenGLSurface : NativeControlHost
{
    private TaskCompletionSource<IntPtr> _handleTaskSource;
    private Thread _gameThread;
    private Engine _engine;
    private volatile bool _isInitialized;

    public new int Width { get; private set; } = 800;
    public new int Height { get; private set; } = 600;

    public OpenGLSurface()
    {
        _handleTaskSource = new TaskCompletionSource<IntPtr>();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        Width = (int)e.NewSize.Width;
        Height = (int)e.NewSize.Height;
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle control)
    {
        Console.WriteLine($"Window handle: 0x{control.Handle.ToInt64():X}");
        Console.WriteLine($"Handle descriptor: {control.HandleDescriptor}");

        if (!_isInitialized)
        {
            InitializeGameThread();
        }

        // Prevents a deadLock if the engine fails to initialize
        var handle = _handleTaskSource.Task.Wait(TimeSpan.FromSeconds(5))
            ? _handleTaskSource.Task.Result
            : throw new TimeoutException("Failed to get engine window handle in time");

        return new PlatformHandle(handle, "X11");
    }

    private void InitializeGameThread()
    {
        _gameThread = new Thread(() =>
        {
            try
            {
                var engineBuilder = new OpenGLBuilder();
                _engine = new Engine(engineBuilder);
                var config = new Configuration
                {
                    IsEmbedded = true,
                    DefaultResolutionWidth = Width,
                    DefaultResolutionHeight = Height,
                };

                _engine.Initialize(config);
                _handleTaskSource.SetResult(_engine.WindowHandle);
                _isInitialized = true;
                _engine.Start();
            }
            catch (Exception ex)
            {
                _handleTaskSource.SetException(ex);
                throw;
            }
        });

        _gameThread.Start();
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        base.DestroyNativeControlCore(control);
        
        // Clean up the engine when the control is destroyed
        if (_engine != null)
        {
            _engine.Shutdown();
            _engine = null;
        }

        if (_gameThread != null && _gameThread.IsAlive)
        {
            _gameThread.Join(TimeSpan.FromSeconds(5));
        }
    }
}