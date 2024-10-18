namespace System03.Core.Scene;

public interface ISceneLifecycle
{
    void OnActivate();
    void OnDeactivate();
    void OnPause();
    void OnResume();
}