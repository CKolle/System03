namespace System03.Input;
public enum Key
{
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K,
    L,
    M,
    N,
    O,
    P,
    Q,
    R,
    S,
    T,
    U,
    V,
    W,
    X,
    Y,
    Z,
    Unknown
}

public interface IKeyboardInputDevice : IInputDevice
{
    public bool IsKeyDown(Key key);
    public bool IsKeyUp(Key key);
}