namespace System03.Input;

public enum MouseButton
{
    LeftClick,
    RightClick
}

public interface IMouseInputDevice : IInputDevice
{
    public bool IsButtonDown(MouseButton button);
    public bool IsButtonUp(MouseButton button);

    public void ShowCursor();
    public void HideCursor();
    public void CenterCursor();
    public int GetMouseX();
    public int GetMouseY();
}