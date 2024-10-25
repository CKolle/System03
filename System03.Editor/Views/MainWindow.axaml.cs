using System;
using System.Threading;
using Avalonia.Controls;
using SixLabors.ImageSharp;
using System03.Core;
using System03.Editor.Controls;
using System03.Core;
using Configuration = System03.Core.Configuration;

namespace System03.Editor.Views;

public partial class MainWindow : Window
{
    private readonly OpenGLSurface _surface;
    public MainWindow()
    {
        InitializeComponent();
        
        // TODO: Research if this is the correct way to do this.
        _surface = this.FindControl<OpenGLSurface>("GameView");
        if (_surface == null)
        {
            throw new InvalidOperationException("Could not find OpenGLSurface control.");
        }
        // Sleep for a bit to allow the window to be created

    }
    
}