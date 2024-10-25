using System.Diagnostics;
using OpenTK.Mathematics;
using System03.Core;
using System03.Core.ECS.Component;
using System03.Core.ECS.System;
using System03.Core.Scene;
using System03.Input;
using System03.Rendering;

namespace System03.Tests.Core;

class MockInputSystem : IInputSystem
{ 
    public void PollInputs()
    {
        // Do nothing
    }

    public T? GetDevice<T>() where T : IInputDevice
    {
        // Do nothing
        return default;
    }
}

class MockRenderer : IRenderer
{
    public void Render()
    {
        // Do nothing
    }

    public void RenderModel(Model model, Matrix4 viewMatrix, Matrix4 projectionMatrix)
    {
        // Do nothing
    }

    public void Initialize()
    {
        // Do nothing
    }
}


class MockScene : BaseScene
{
}

// Takes in a delegate that will be called when the renderer is called. The action does not
public class CountRenderer(Action onRender) : IRenderSystem
{
    public void Render(ComponentManager componentManager, IRenderer renderer)
    {
        onRender();
    }
}

public class GameLoopTests
{
    // Tests with common frame rates
    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(144)]
    [InlineData(240)]
    public void Update_RunsAtTargetFrameRate(int targetFPS)
    {
        // Arrange
        // Callback to count frames
        
        var renderer = new MockRenderer();
        var inputSystem = new MockInputSystem();
        var scene = new MockScene();
        var gameLoop = new GameLoop(renderer, inputSystem, scene);
        const int testDurationMs = 2000;
        int frameCount = 0;
        var renderSystem = new CountRenderer(() => frameCount++);
        var stopWatch = new Stopwatch();
        scene.AddRenderSystem(renderSystem);
        
        // Act
        // Make a new thread to run the game loop
        var gameLoopTask = new Task(() => gameLoop.Start(targetFPS));
        stopWatch.Start();
        gameLoopTask.Start();
        // We spin lock to wait the test duration
        while (stopWatch.ElapsedMilliseconds < testDurationMs)
        {
           // Spin lock
        }
        gameLoop.Stop();
        
        // Now we divide the frame count by the test duration to get the frame rate
        frameCount = frameCount * 1000 / testDurationMs;
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
        // Arrange
        var renderer = new MockRenderer();
        var inputSystem = new MockInputSystem();
        var scene = new MockScene();
        var _gameLoop = new GameLoop(renderer, inputSystem, scene);
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _gameLoop.Start(targetFPS));
    }
}