namespace System03.Windowing.Abstractions;

public interface IWindowSystem : IDisposable
{
    public record WindowConfig(
        int Width = 800,
        int Height = 600,
        bool IsEmbedded = false
    );
    IntPtr NativeDisplay();
    IntPtr NativeHandle();
    bool ShouldClose();
}