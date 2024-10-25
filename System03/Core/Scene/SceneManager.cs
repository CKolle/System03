namespace System03.Core.Scene;

public class SceneManager
{
    private readonly Dictionary<string, BaseScene> _scenes = new();
    private BaseScene? _activeScene;

    public void RegisterScene(string name, BaseScene baseScene)
    {
        _scenes.Add(name, baseScene);
    }

    public void SetActiveScene(string name)
    {
        if (!_scenes.TryGetValue(name, out var scene))
            throw new ArgumentException($"Scene with name {name} not found.");
        _activeScene?.OnDeactivate();
        _activeScene = scene;
        _activeScene.OnActivate();
    }

    public void PauseActiveScene()
    {
        _activeScene?.OnPause();
    }
    
    public void ResumeActiveScene()
    {
        _activeScene?.OnResume();
    }
}