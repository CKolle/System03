using System.Diagnostics;
using System03.Input;
using System03.Rendering;

namespace System03.Core;

public class GameLoop
{

    private readonly IRenderer _renderer;
    private readonly IInputSystem _inputSystem;
    private int _targetFPS;
    private Thread? _gameThread;
    private bool _isRunning;
    
    private readonly Stopwatch _stopwatch = new();
    
    public event EventHandler<UpdateEventArgs> OnUpdate = delegate { };

    public GameLoop(IRenderer renderer, IInputSystem inputSystem)
    {
            _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _inputSystem = inputSystem ?? throw new ArgumentNullException(nameof(inputSystem));
    }
    
    
    public void Start(int targetFPS)
    {
        if (targetFPS <= 0)
            throw new ArgumentException("Target FPS must be greater than 0.");

        _targetFPS = targetFPS;
        _isRunning = true;
        
        _gameThread = new Thread(RunGameLoop);
        _stopwatch.Start();
        _gameThread.Start();
    }
    
    public void Stop()
    {
        _isRunning = false;

        _gameThread?.Join();
        _stopwatch.Stop();
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

            _inputSystem.PollInput();
            
            accumulator += frameTime;
            
            
            while (accumulator >= dt)
            {
                OnUpdate.Invoke(this, new UpdateEventArgs(dt));
                accumulator -= dt;
            }
            

            _renderer.Render();
        }
    }

}

public class UpdateEventArgs(double deltaTime) : EventArgs
{
    public double DeltaTime { get; } = deltaTime;
}