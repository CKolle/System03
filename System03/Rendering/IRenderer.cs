using OpenTK.Mathematics;

namespace System03.Rendering;

public interface IRenderer
{
    public void Render();
    
    // TODO change this the renderer should have its own camera
    public void RenderModel(Model model, Matrix4 viewMatrix, Matrix4 projectionMatrix);
    // We can to this because the renderer owns a graphics context which
    // The graphics context gets initialized with a windowSystem
    public void Initialize();
}