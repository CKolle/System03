using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;

namespace System03.Rendering.OpenGL;

public class Shader
{
    private readonly int _program;
    private readonly Dictionary<string, int> _uniformLocations = new();
    
    /// <summary>
    /// Creates a new Shader program from the given vertex and fragment sources
    /// </summary>
    /// <param name="vertexSource">The vertex shader source</param>
    /// <param name="fragmentSource">The fragment shader source</param>
    public Shader(string vertexSource, string fragmentSource)
    {
        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexSource);
        GL.CompileShader(vertexShader);

        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentSource);
        GL.CompileShader(fragmentShader);
        
        // Create the program
        _program = GL.CreateProgram();
        GL.AttachShader(_program, vertexShader);
        GL.AttachShader(_program, fragmentShader);
        GL.LinkProgram(_program);
        
        // Clean up
        GL.DetachShader(_program, vertexShader);
        GL.DetachShader(_program, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
        
        // Get uniform locations
        GL.GetProgram(_program, GetProgramParameterName.ActiveUniforms, out var numUniforms);
        
        for (var i = 0; i < numUniforms; i++)
        {
            var name = GL.GetActiveUniform(_program, i, out _, out _);
            var location = GL.GetUniformLocation(_program, name);
            _uniformLocations.Add(name, location);
        }
    }
    
    public void Use()
    {
        GL.UseProgram(_program);
    }

    // Uniform setters with type safety
    public void SetInt(string name, int data)
    {
        if (_uniformLocations.TryGetValue(name, out int location))
            GL.Uniform1(location, data);
    }

    public void SetFloat(string name, float data)
    {
        if (_uniformLocations.TryGetValue(name, out int location))
            GL.Uniform1(location, data);
    }

    public void SetMatrix4(string name, Matrix4 data)
    {
        if (_uniformLocations.TryGetValue(name, out int location))
            GL.UniformMatrix4(location, false, ref data);
    }

    public void SetVector3(string name, Vector3 data)
    {
        if (_uniformLocations.TryGetValue(name, out int location))
            GL.Uniform3(location, data);
    }

    public void SetVector2(string name, Vector2 data)
    {
        if (_uniformLocations.TryGetValue(name, out int location))
            GL.Uniform2(location, data);
    }

}