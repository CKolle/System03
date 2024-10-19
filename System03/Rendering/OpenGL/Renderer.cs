using System03.Core;
using System03.Windowing.Abstractions;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System03.Rendering.Graphics;

namespace System03.Rendering.OpenGL;

public class Renderer : IRenderer
{
    public Renderer(IGraphicsContext contextBase)
    {
        contextBase.MakeCurrent();
        GL.LoadBindings(contextBase);
    }
    public void Render()
    {
        // Lets just make the screen red for no
        GL.ClearColor(1.0f, 0.0f, 0.0f, 1.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
    }
}