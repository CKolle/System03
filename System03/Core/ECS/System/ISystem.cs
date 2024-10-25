using System03.Core.ECS.Component;
using System03.Rendering;

namespace System03.Core.ECS.System;

public interface ISystem
{
    void Update(ComponentManager componentManager, double deltaTime);
}

public interface IRenderSystem 
{
    void Render(ComponentManager componentManager, IRenderer renderer);
}