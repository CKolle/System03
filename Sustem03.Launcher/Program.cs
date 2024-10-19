// See https://aka.ms/new-console-template for more information

using System03.Rendering.Graphics;
using System03.Rendering.OpenGL;
using System03.Vinduer;
using System03.Windowing.Abstractions;


var window = new WindowBuilder()
    .WithDimensions(800, 600)
    .WithBackend(WindowBackend.X11)
    .Build();

var context = new EGLContext();
context.Initialize(window);
context.MakeCurrent();
var renderer = new Renderer(context);

while (true)
{
    renderer.Render();
    context.SwapBuffers();
}




