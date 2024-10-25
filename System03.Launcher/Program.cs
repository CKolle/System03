// See https://aka.ms/new-console-template for more information

using System03.Core;
using System03.Rendering.Graphics;
using System03.Rendering.OpenGL;
using System03.Vinduer;
using System03.Windowing.Abstractions;


/*var window = new WindowBuilder()
    .WithDimensions(800, 600)
    .WithBackend(WindowBackend.X11)
    .Build();



var context = new EGLContext();
context.Initialize(window);
context.MakeCurrent();
var renderer = new Renderer(context);*/
// Sleep for a bit to allow the context to initialize

var engineBuilder = new OpenGLBuilder();
var engine = new Engine(engineBuilder);
var config = new Configuration
{
    DefaultResolutionWidth = 1920,
    DefaultResolutionHeight = 1200,
    TargetFPS = 144,
    IsEmbedded = false
    
};
engine.Initialize(config);
engine.Start();


/*
while (true)
{
    renderer.Render();
    context.SwapBuffers();
}
*/




