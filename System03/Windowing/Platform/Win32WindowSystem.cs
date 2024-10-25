using System03.Input;
using System03.Windowing.Abstractions;

namespace System03.Windowing.Platform;

public class Win32WindowSystem : IWindowSystem
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public IntPtr NativeDisplay()
    {
        throw new NotImplementedException();
    }

    public IntPtr NativeHandle()
    {
        throw new NotImplementedException();
    }

    public bool ShouldClose()
    {
        throw new NotImplementedException();
    }

    public List<IInputDevice> GetInputDevices()
    {
        throw new NotImplementedException();
    }

    public List<IPollGroup> GetPollGroups()
    {
        throw new NotImplementedException();
    }
}