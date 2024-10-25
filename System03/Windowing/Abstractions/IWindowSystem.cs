using System03.Input;

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
    
    /// <summary>
    /// Gets a list of input devices that are available to the window system.
    /// If there are any input systems.
    /// </summary>
    public List<IInputDevice> GetInputDevices();
    /// <summary>
    /// Gets a list of poll groups that are available to the window system.
    /// Poll groups are used to poll multiple devices at once. For window systems that does it that way.
    /// You can also make a poll group for each device if the window system does not support polling multiple devices at once.
    /// </summary>
    public List<IPollGroup> GetPollGroups();
}