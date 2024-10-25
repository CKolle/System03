namespace System03.Input;


/// <summary>
/// An input system that can be used to poll devices for input
/// </summary>
public interface IInputSystem
{
    /// <summary>
    /// Polls all devices for input
    /// </summary>
    public void PollInputs();
    
    /// <summary>
    /// Gets the device with the given type
    /// </summary>
    public T? GetDevice<T>() where T : IInputDevice;
    
}

/// <summary>
/// Represents a group of devices that can be polled together
/// Each device must be in a poll group. And they can be in a group for themselves
/// </summary>
public interface IPollGroup
{
    /// <summary>
    /// Polls all devices in the group
    /// </summary>
    public void PollGroup();
}

public class InputSystem : IInputSystem
{
    private readonly List<IInputDevice> _devices;
    private readonly List<IPollGroup> _pollGroups;
    public InputSystem(List<IInputDevice> devices, List<IPollGroup> pollGroups)
    {
        _devices = devices;
        _pollGroups = pollGroups;
    }
    public void PollInputs()
    {
        foreach (var group in _pollGroups)
        {
            group.PollGroup();
        }
    }
    public T? GetDevice<T>() where T : IInputDevice
    {
        return (T)_devices.Find(d => d is T);
    }
}
