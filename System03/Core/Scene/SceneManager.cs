namespace System03.Core.Scene;

public class SceneManager
{
    private readonly Dictionary<string, Scene> _scenes = new();
    private Scene? _activeScene;

    public void RegisterScene(string name, Scene scene)
    {
        _scenes.Add(name, scene);
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