using System03.Core;
using System03.Tests.TestHelpers.Mocks.Input;
using System03.Tests.TestHelpers.Mocks.Rendering;

namespace System03.Tests.Core;

public class GameLoopTests
{
    private readonly GameLoop _gameLoop;

    public GameLoopTests()
    {
        var renderer = new MockRenderer();
        var inputSystem = new MockInputSystem();
        _gameLoop = new GameLoop(renderer, inputSystem);
    }

    [Fact]
    public async Task Update_RunsAtTargetFrameRate()
    {
        // Arrange
        const int targetFPS = 60;
        const int testDurationMs = 1000;
        var frameCount = 0;
        _gameLoop.OnUpdate += (_, _) => frameCount++;        
        // Act
        _gameLoop.Start(targetFPS);
        await Task.Delay(testDurationMs);
        _gameLoop.Stop();
        
        // Assert
        const int tolerance = 5;
        Assert.InRange(frameCount, targetFPS - tolerance, targetFPS + tolerance); 
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Start_InvalidTargetFPS_ThrowsException(int targetFPS)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _gameLoop.Start(targetFPS));
    }
}