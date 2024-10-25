using System03.Core.ECS;
using System03.Core.ECS.Component;
using System03.Core.ECS.System;
using System03.Rendering;

namespace System03.Core.Scene;

public abstract class BaseScene : ISceneLifecycle
{
    public readonly ComponentManager ComponentManager = new();
    private readonly List<ISystem> _preRenderSystems = [];
    private readonly List<ISystem> _postRenderSystems = [];

    private readonly List<IRenderSystem> _renderSystems = [];
    // TODO This is bad, but I don't care right now. Should use a manager for this.
    // TODO We should use versioning of entity ids to avoid reusing them.
    private int _nextEntityId;
    public Entity CreateEntity()
    {
        return new Entity(_nextEntityId++);
    }
    
    public void AddPreRenderSystem(ISystem system)
    {
        _preRenderSystems.Add(system);
    }
    
    public void RemovePreRenderSystem(ISystem system)
    {
        _preRenderSystems.Remove(system);
    }

    public void UpdatePreRender(double deltaTime)
    {
        foreach (var system in _preRenderSystems)
        {
            system.Update(ComponentManager, deltaTime);
        }
    }
    
    public void AddPostRenderSystem(ISystem system)
    {
        _postRenderSystems.Add(system);
    }
    
    public void RemovePostRenderSystem(ISystem system)
    {
        _postRenderSystems.Remove(system);
    }
    
    public void UpdatePostRender(double deltaTime)
    {
        foreach (var system in _postRenderSystems)
        {
            system.Update(ComponentManager, deltaTime);
        }
    }
    
    public void AddRenderSystem(IRenderSystem system)
    {
        _renderSystems.Add(system);
    }
    
    public void RemoveRenderSystem(IRenderSystem system)
    {
        _renderSystems.Remove(system);
    }
    public void Render(IRenderer renderer)
    {
        foreach (var system in _renderSystems)
        {
            system.Render(ComponentManager, renderer);
        }
    }
    
    // Allows the scene to initialize any resources it needs
    public virtual void OnActivate() {}
    public virtual void OnDeactivate() {}
    public virtual void OnPause() {}
    public virtual void OnResume() {}
}