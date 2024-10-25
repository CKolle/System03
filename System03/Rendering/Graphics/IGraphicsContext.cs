using OpenTK;
using System03.Windowing.Abstractions;

namespace System03.Rendering.Graphics;

/* TODO: This is bad, we should not be using OpenTK here, but I don't care. Because we may have vulkan or opengl
    Make a separate IOpenGLContext  which should have OpenGL specifics
 */
public interface IGraphicsContext : IDisposable, IBindingsContext
{
    void Initialize();
    void MakeCurrent();
    void SwapBuffers();
}