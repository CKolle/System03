using System03.Core.Scene;
using System03.Input;
using System03.Rendering;
using System03.Rendering.Graphics;
using System03.Rendering.OpenGL;
using System03.Resources;
using System03.Vinduer;
using System03.Windowing.Abstractions;

namespace System03.Core;

public interface IEngineComponents
{
    public IWindowSystem WindowSystem { get; }
    public IGraphicsContext GraphicsContext { get; }
    public IRenderer Renderer { get; }
    public IInputSystem InputSystem { get; }
    public GameLoop GameLoop { get; }
    public IResourceManager ResourceManager { get; }
}

public class EngineComponents(
    IWindowSystem windowSystem,
    IGraphicsContext graphicsContext,
    IRenderer renderer,
    IInputSystem inputSystem,
    GameLoop gameLoop,
    IResourceManager resourceManager) : IEngineComponents
{
    public IWindowSystem WindowSystem { get; } = windowSystem;
    public IGraphicsContext GraphicsContext { get; } = graphicsContext;
    public IRenderer Renderer { get; } = renderer;
    public IInputSystem InputSystem { get; } = inputSystem;
    public GameLoop GameLoop { get; } = gameLoop;
    public IResourceManager ResourceManager { get; } = resourceManager;
}


public interface IEngineBuilder
{
    public IEngineComponents BuildComponents(Configuration config);
}

public class OpenGLBuilder : IEngineBuilder
{
    // TODO: Find a way to avoid passing the first scene here -- but I don't care right now
    // Maybe rename it initalScene? I will have to think about this.
    public IEngineComponents BuildComponents(Configuration config)
    {
        var resourceManager = new ResourceManager();

        var windowSystem = new WindowBuilder()
            .WithDimensions(config.DefaultResolutionWidth, config.DefaultResolutionHeight)
            .IsEmbedded(config.IsEmbedded)
            .Build();

        var graphicsContext = new EGLContext(windowSystem);
        var renderer = new Renderer(graphicsContext);
        var inputSystem = new InputSystem(windowSystem.GetInputDevices(), windowSystem.GetPollGroups());
        // TODO - This is bad, but I don't care right now
        var firstScene = new Scene3D(inputSystem);
        var gameLoop = new GameLoop(renderer, inputSystem, firstScene);

        return new EngineComponents(windowSystem, graphicsContext, renderer, inputSystem, gameLoop, resourceManager);
    }
}