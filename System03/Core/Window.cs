namespace System03.Core;

public interface IWindow 
{
    uint Width { get; }
    uint Height { get; }
    bool IsVSync { get; }
    void SwapBuffers();
    void MakeCurrentContext();
    bool ShouldClose { get; }
    event Action<uint, uint> Resize;
}



