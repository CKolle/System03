namespace System03.Core;

public class Configuration
{
    private int _targetFPS = 60;
    public int TargetFPS {
        get => _targetFPS;
        set
        {
            if (value < 1)
            {
                throw new ArgumentException("Target FPS must be greater than 0.", nameof(value));
            }

            _targetFPS = value;
        }
    }
    public int DefaultResolutionWidth { get; set; } = 1920;
    public int DefaultResolutionHeight { get; set; } = 1080;
    public bool VSync { get; set; } = true;
    
    // TODO Move this out to the enginebuilder
    public bool IsEmbedded { get; init; }

    public void SaveToFile(string path)
    {
        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var writer = new BinaryWriter(fileStream);
        writer.Write(TargetFPS);
        writer.Write(DefaultResolutionWidth);
        writer.Write(DefaultResolutionHeight);
        writer.Write(VSync);
    }

    public static Configuration LoadFromFile(string path)
    {
        
        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        using var reader = new BinaryReader(fileStream);
        
        return new Configuration
        {
            TargetFPS = reader.ReadInt32(),
            DefaultResolutionWidth = reader.ReadInt32(),
            DefaultResolutionHeight = reader.ReadInt32(),
            VSync = reader.ReadBoolean()
        };
    }
}