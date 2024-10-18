using System03.Core.ECS;
using System03.Core.ECS.Component;
using System03.Core.ECS.System;

namespace System03.Core.Scene;

public abstract class Scene : ISceneLifecycle
{
    protected readonly ComponentManager ComponentManager = new();
    protected readonly List<ISystem> Systems = [];
    // TODO This is bad, but I don't care right now. Should use a manager for this.
    private int _nextEntityId;

    public Entity CreateEntity()
    {
        return new Entity(_nextEntityId++);
    }
    
    public void AddSystem(ISystem system)
    {
        Systems.Add(system);
    }
    
    public void RemoveSystem(ISystem system)
    {
        Systems.Remove(system);
    }

    public void Update(double deltaTime)
    {
        foreach (var system in Systems)
        {
            system.Update(ComponentManager, deltaTime);
        }
    }

    public virtual void OnActivate() {}
    public virtual void OnDeactivate() {}
    public virtual void OnPause() {}
    public virtual void OnResume() {}
}