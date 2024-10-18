using System03.Core;

namespace System03.Rendering.OpenGL;

public class Renderer(IWindow window, IOpenGLContext context) : IRenderer
{
    private readonly IWindow _window = window;
    private readonly IOpenGLContext _context = context;

    public void Render()
    {
        throw new NotImplementedException();
    }
}