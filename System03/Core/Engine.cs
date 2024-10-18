using System03.Resources;

namespace System03.Core;

public class Engine
{
    public Configuration? Configuration { get; private set; }
    public bool IsInitialized { get; private set; }
    public IResourceManager? ResourceManager { get; private set; }
    public void Initialize(Configuration config)
    {
        if (IsInitialized)
            throw new InvalidOperationException("Engine is already initialized.");
        
        ResourceManager = new ResourceManager();
        Configuration = config;
        IsInitialized = true;
    }
    
    public void Shutdown()
    {
        
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}