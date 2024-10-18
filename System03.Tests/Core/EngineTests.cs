using System03.Core;

namespace System03.Tests.Core;

public class EngineTests
{
    private readonly Engine _engine = new Engine();

    [Fact]
    public void Initialize_WidthValidConfig_EngineInitializes()
    {
        // Arrange
        var config = new Configuration { TargetFPS = 60 } ;
        
        // Act
        _engine.Initialize(config);
        
        // Assert
        Assert.True(_engine.IsInitialized);
        Assert.NotNull(_engine.ResourceManager);
    }
    
    [Fact]
    public void Initialize_CalledTwice_ThrowsException()
    {
        // Arrange
        var config = new Configuration { TargetFPS = 60 } ;
        _engine.Initialize(config);
        
        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _engine.Initialize(config));
    }

    public void Dispose()
    {
        _engine?.Shutdown();
    }
}