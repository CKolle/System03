namespace System03.Core;


public class Engine
{
    private readonly IEngineBuilder _engineBuilder;
    private IEngineComponents _components;
    public bool IsInitialized { get; private set; }
    public Configuration Configuration { get; private set; }

    
    // For embedded windows that need access to the window handle
    public IntPtr WindowHandle => _components?.WindowSystem.NativeHandle() 
                                  ?? throw new InvalidOperationException("Engine not initialized");
    public Engine(IEngineBuilder engineBuilder)
    {
        _engineBuilder = engineBuilder;
    }
    
    public void Initialize(Configuration config)
    {
        if (IsInitialized)
            throw new InvalidOperationException("Engine is already initialized.");
        Configuration = config;
        _components = _engineBuilder.BuildComponents(config);
        
        // Note due to threading we can't initialize the renderer here
        // It must be done on the game thread
        // Other components can be initialized here
        IsInitialized = true;
    }

    public void Start()
    {
        if (!IsInitialized)
            throw new InvalidOperationException("Engine is not initialized.");
        
        _components.GameLoop.Start(Configuration.TargetFPS);
    }
    public void Shutdown()
    {
        // TODO: Implement order may matter maybe?
    }
    
}