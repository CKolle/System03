using System.Diagnostics;
using System03.Core.Scene;
using System03.Input;
using System03.Rendering;

namespace System03.Core;

public class GameLoop
{

    private readonly IRenderer _renderer;
    private readonly IInputSystem _inputSystem;
    private int _targetFPS;
    private Thread? _gameThread;

    private BaseScene _currentScene;
    // Allows us to queue up the next scene to be loaded
    private BaseScene? _nextScene;
    
    // We don't want the compiler to optimize this variable
    private volatile bool _isRunning;
    
    private readonly Stopwatch _stopwatch = new();
    
    public GameLoop(IRenderer renderer, IInputSystem inputSystem, BaseScene scene)
    {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _inputSystem = inputSystem ?? throw new ArgumentNullException(nameof(inputSystem));
            _currentScene = scene ?? throw new ArgumentNullException(nameof(scene));
    }


    public void Start(int targetFPS)
    {
        if (targetFPS <= 0)
            throw new ArgumentException("Target FPS must be greater than 0.");
        
        _targetFPS = targetFPS;
        _isRunning = true;

        _gameThread = new Thread(() =>
        {
            _stopwatch.Start();
            // We need to initialize the renderer on the game thread
            // Else we get a black screen
            _renderer.Initialize();
            _currentScene.OnActivate();
            RunGameLoop();
        });
        _gameThread.Start();
    }
    
    public void Stop()
    {
        _isRunning = false;

        _gameThread?.Join();
        _stopwatch.Stop();
    }
    
    public void SetScene(BaseScene scene)
    {
        _nextScene = scene;
    }

    private void RunGameLoop()
    {
        double dt = 1.0f / _targetFPS; // Time in frame per seconds
        double accumulator = 0.0f;
        
        var previousTime = _stopwatch.ElapsedMilliseconds;

        while (_isRunning)
        {

            var currentTime = _stopwatch.ElapsedMilliseconds;
            double frameTime = (currentTime - previousTime) / 1000.0f;
            previousTime = currentTime;

            _inputSystem.PollInputs();
            
            accumulator += frameTime;
            
            while (accumulator >= dt)
            {
                _currentScene.UpdatePreRender(dt);
                accumulator -= dt;
            }
            
            
            _currentScene.Render(_renderer);
            
            _currentScene.UpdatePostRender(frameTime);
            
            // Thread sleep is not accurate enough to keep the target frame rate
            // We should just spinlock instead. Tried to use Thread.Sleep but it was not accurate enough
            // Got 165 fps instead of 144. This works better but if anyone has a better solution please let me know
            // Christopher 
            while (_stopwatch.ElapsedMilliseconds - currentTime < dt * 1000)
            {
                // Spinlock
            }
        }
    }
}
