using System03.Core.ECS.Component;

namespace System03.Core.ECS.System;

public interface ISystem
{
    void Update(ComponentManager componentManager, double deltaTime);
}