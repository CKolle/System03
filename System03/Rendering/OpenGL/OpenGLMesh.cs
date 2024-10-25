using OpenTK.Graphics.ES30;
using OpenTK.Mathematics;
using System03.FileFormats;
using System.Runtime.InteropServices;

namespace System03.Rendering.OpenGL;

// We are targeting OpenGL ES 3.2 (via OpenTK)
public class OpenGLMesh : IMesh, IDisposable
{
    // Element buffer object
    private readonly int _vao;
    private readonly int _vbo;
    private readonly int _ebo;
    public int VertexCount { get; private set;}

    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 TexCoord;
        public Vector3 Normal;

        public static readonly int SizeInBytes = Marshal.SizeOf<Vertex>();
    }

    public OpenGLMesh(ObjFile objFile)
    {
        var vertices = new List<Vertex>();
        var indices = new List<int>();
        
        // Process each face and create unique vertices
        // TODO - There is a bug here.
        // Face1 (Material A) -> indices 0-2
        // Face2 (Material B) -> indices 3-5
        // Face3 (Material A) -> indices 6-8
        // We should ensure that we load the faces in order of material groups.
        // Else we will use the wrong material for the wrong face... TODO - Fix this when I care.
        foreach (var face in objFile.Faces)
        {
            for (var i = 0; i < face.VertexIndices.Length; i++)
            {
                var vertex = new Vertex
                {
                    Position = new Vector3(
                        objFile.GeometricVertices[face.VertexIndices[i]].X,
                        objFile.GeometricVertices[face.VertexIndices[i]].Y,
                        objFile.GeometricVertices[face.VertexIndices[i]].Z
                    )
                };

                if (face.TextureCoordinateIndices != null)
                {
                    var texCoord = objFile.TextureCoordinates[face.TextureCoordinateIndices[i]];
                    vertex.TexCoord = new Vector2(texCoord.U, texCoord.V);
                }

                if (face.NormalIndices != null)
                {
                    var normal = objFile.VertexNormals[face.NormalIndices[i]];
                    vertex.Normal = new Vector3(normal.X, normal.Y, normal.Z);
                }

                vertices.Add(vertex);
                indices.Add(indices.Count);
            }
        }

        VertexCount = indices.Count;
        
        // Proud of this one :)
        // Buffer layout:
        // Vertex Array Object (VAO)
        // ├── Vertex Buffer Object (VBO)
        // │   ├── Position (vec3) - location 0
        // │   ├── TexCoord (vec2) - location 1
        // │   └── Normal (vec3) - location 2
        // └── Element Buffer Object (EBO)

        // Generate VAO
        GL.GenVertexArrays(1, out _vao);
        GL.BindVertexArray(_vao);

        // Generate and bind VBO
        GL.GenBuffers(1, out _vbo);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Count * Vertex.SizeInBytes, vertices.ToArray(), BufferUsageHint.StaticDraw);

        // Generate and bind EBO
        GL.GenBuffers(1, out _ebo);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(int), indices.ToArray(), BufferUsageHint.StaticDraw);

        // Set vertex attributes
        // Position
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);
        
        // Texture coordinates
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, Marshal.OffsetOf<Vertex>("TexCoord").ToInt32());
        
        // Normals
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, Marshal.OffsetOf<Vertex>("Normal").ToInt32());

        // Unbind the VAO
        GL.BindVertexArray(0);
    }
    
    private static void CheckGLError(string action)
    {
        var error = GL.GetError();
        if (error != ErrorCode.NoError)
        {
            throw new Exception($"OpenGL Error during {action}: {error}");
        }
    }
    public void Bind()
    {
        GL.BindVertexArray(_vao);
        //CheckGLError("Bind VAO");
    }

    public void Unbind()
    {
        GL.BindVertexArray(0);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(_vbo);
        GL.DeleteBuffer(_ebo);
        GL.DeleteVertexArray(_vao);
    }
}