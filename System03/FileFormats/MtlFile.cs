using System.Globalization;

namespace System03.FileFormats;
/// <summary>
/// Represent a Wavefront MTL file format parser and container
/// </summary>
public class MtlFile
{
    public readonly record struct Color(float R, float G, float B);
    
    public record Material
    {
        public string Name { get; init; }
        public Color Ambient { get; internal set; }
        public Color Diffuse { get; internal set; }
        public Color Specular { get; internal set; }
        public Color Emissive { get; internal set; }
        public float Shininess { get; internal set; }
        public string? DiffuseTextureMap { get; internal set; }
    }
    // Current material being parsed
    private Material? _currentMaterial;
    private readonly List<Material> _materials = [];
    public IReadOnlyList<Material> Materials => _materials;
    
    /// <summary>
    /// Creates a new MtlFile instance and loads data from the specified file path
    /// </summary>
    /// <param name="path">Path to the MTL file</param>
    /// <exception cref="FileNotFoundException">Thrown when the file cannot be found</exception>
    /// <exception cref="MtlParseException">Thrown when the file contains invalid data</exception>
    public MtlFile(string path)
    {
        using var stream = File.OpenRead(path);
        Load(stream);
    }

    /// <summary>
    /// Creates a new MtlFile instance and loads data from the specified stream
    /// </summary>
    /// <param name="stream">Stream containing MTL file data</param>
    /// <exception cref="MtlParseException">Thrown when the stream contains invalid data</exception>
    public MtlFile(Stream stream)
    {
        Load(stream);
    }

    private void Load(Stream stream)
    {
        using var reader = new StreamReader(stream);
        string? line;
        var lineNumber = 0;
        try
        {
            while ((line = reader.ReadLine()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                    continue;
                
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;
                
                ParseLine(parts);
            }
            // Add the last material
            if (_currentMaterial != null)
                _materials.Add(_currentMaterial);

        }
        catch (Exception ex) when (ex is not MtlParseException)
        {
            throw new MtlParseException($"Error parsing MTL file at line {lineNumber}: {ex.Message}", ex);
        }
    }

    private void ParseLine(string[] parts)
    {
        switch (parts[0].ToLowerInvariant())
        {
            case "newmtl":
                if (_currentMaterial != null)
                    _materials.Add(_currentMaterial);
                // we can have multiple words in the material name
                _currentMaterial = new Material { Name = parts.Skip(1).Aggregate((a, b) => $"{a} {b}") };
                break;
            case "ka":
                if (_currentMaterial == null)
                    throw new MtlParseException("Ambient color defined before material");
                _currentMaterial.Ambient = ParseColor(parts);
                break;
            case "kd":
                if (_currentMaterial == null)
                    throw new MtlParseException("Diffuse color defined before material");
                _currentMaterial.Diffuse = ParseColor(parts);
                break;
            case "ks":
                if (_currentMaterial == null)
                    throw new MtlParseException("Specular color defined before material");
                _currentMaterial.Specular = ParseColor(parts);
                break;
            case "ke":
                if (_currentMaterial == null)
                    throw new MtlParseException("Emissive color defined before material");
                _currentMaterial.Emissive = ParseColor(parts);
                break;
            case "ns":
                if (_currentMaterial == null)
                    throw new MtlParseException("Shininess defined before material");
                if (parts.Length < 2)
                    throw new MtlParseException("Shininess must have a value");
                _currentMaterial.Shininess = float.Parse(parts[1], CultureInfo.InvariantCulture);
                break;
            case "map_kd":
                if (_currentMaterial == null)
                    throw new MtlParseException("Diffuse texture map defined before material");
                if (parts.Length < 2)
                    throw new MtlParseException("Diffuse texture map must have a value");
                _currentMaterial.DiffuseTextureMap = parts[1];
                break;
        }
    }

    private static Color ParseColor(string[] parts)
    {
        if (parts.Length < 4)
            throw new MtlParseException("Color must have at least 3 components");
        return new Color(
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture),
            float.Parse(parts[3], CultureInfo.InvariantCulture)
        );
    }
}

public class MtlParseException : Exception
{
    public MtlParseException(string message) : base (message) { }
    public MtlParseException(string message, Exception innerException) : base(message, innerException) { }
}