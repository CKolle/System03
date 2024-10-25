namespace System03.Rendering;

public interface IMesh
{
    int VertexCount { get; }
    void Bind();
    void Unbind();
}