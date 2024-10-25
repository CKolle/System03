using System.Globalization;

namespace System03.FileFormats;

public class ObjFile
{
    public readonly record struct Vertex(float X, float Y, float Z);

    public readonly record struct TextureCoordinate(float U, float V, float W = 0.0f);

    public readonly record struct Face
    {
        public required int[] VertexIndices { get; init; }
        public int[]? TextureCoordinateIndices { get; init; }
        public int[]? NormalIndices { get; init; }
        public string? MaterialName { get; init; }
    }

    // Collections for storing OBJ data
    private readonly List<Vertex> _geometricVertices = [];
    private readonly List<TextureCoordinate> _textureCoordinates = [];
    private readonly List<Vertex> _vertexNormals = [];
    private readonly List<Vertex> _parameterSpaceVertices = [];
    private readonly List<Face> _faces = [];
    private readonly Dictionary<string, List<Face>> _materialGroups = []; 

    // Public read-only access to the data
    public IReadOnlyList<Vertex> GeometricVertices => _geometricVertices;
    public IReadOnlyList<TextureCoordinate> TextureCoordinates => _textureCoordinates;
    public IReadOnlyList<Vertex> VertexNormals => _vertexNormals;
    
    public IReadOnlyList<Vertex> ParameterSpaceVertices => _parameterSpaceVertices;
    public IReadOnlyList<Face> Faces => _faces;

    public IReadOnlyDictionary<string, IReadOnlyList<Face>> MaterialGroups =>
        _materialGroups.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<Face>)kvp.Value
        );

    // Additional metadata
    // TODO: These should not be public getter why would you need to get the last loaded object info? But I am tired right now
    public string? ObjectName { get; private set; }
    public string? MaterialLibrary { get; private set; }
    public string? CurrentGroup { get; private set; }
    private string? _currentMaterial; // Changed to private field
    public string? CurrentMaterial => _currentMaterial; // Public getter only

    // TODO - Probably change this to a path type or something (dont' know what is available in C#)
    public ObjFile(string path)
    {
        using var stream = File.OpenRead(path);
        Load(stream);
    }

    public ObjFile(Stream stream)
    {
        Load(stream);
    }

    private void Load(Stream stream)
    {
        using var reader = new StreamReader(stream);
        string? line;
        int lineNumber = 0;

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
        }
        catch (Exception ex) when (ex is not ObjParseException)
        {
            throw new ObjParseException($"Error parsing OBJ file at line {lineNumber}: {ex.Message}", ex);
        }
    }

    private void ParseLine(string[] parts)
    {
        switch (parts[0].ToLowerInvariant())
        {
            case "v":
                ParseVertex(parts);
                break;
            case "vt":
                ParseTextureCoordinate(parts);
                break;
            case "vn":
                ParseNormal(parts);
                break;
            case "vp":
                ParseParameterSpaceVertex(parts);
                break;
            case "f":
                ParseFace(parts);
                break;
            case "o":
                ObjectName = parts.Length > 1 ? parts[1] : null;
                break;
            case "g":
                CurrentGroup = parts.Length > 1 ? parts[1] : null;
                break;
            case "mtllib":
                // Could have the matial library split into multiple parts so we join them
                MaterialLibrary = parts.Skip(1).Aggregate((a, b) => $"{a} {b}");
                break;
            case "usemtl":
                if (parts.Length > 1)
                {
                    _currentMaterial = parts[1];
                    // Ensure the material group exists
                    if (!_materialGroups.ContainsKey(_currentMaterial))
                    {
                        _materialGroups[_currentMaterial] = [];
                    }
                }

                break;
        }
    }

    private void ParseVertex(string[] parts)
    {
        if (parts.Length < 4)
            throw new ObjParseException("Invalid vertex definition");

        _geometricVertices.Add(new Vertex(
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture),
            float.Parse(parts[3], CultureInfo.InvariantCulture)
        ));
    }

    private void ParseTextureCoordinate(string[] parts)
    {
        if (parts.Length < 3)
            throw new ObjParseException("Invalid texture coordinate definition");

        _textureCoordinates.Add(new TextureCoordinate(
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture),
            parts.Length > 3 ? float.Parse(parts[3], CultureInfo.InvariantCulture) : 0.0f
        ));
    }

    private void ParseNormal(string[] parts)
    {
        if (parts.Length < 4)
            throw new ObjParseException("Invalid normal definition");

        _vertexNormals.Add(new Vertex(
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture),
            float.Parse(parts[3], CultureInfo.InvariantCulture)
        ));
    }

    private void ParseParameterSpaceVertex(string[] parts)
    {
        if (parts.Length < 4)
            throw new ObjParseException("Invalid parameter space vertex definition");

        _parameterSpaceVertices.Add(new Vertex(
            float.Parse(parts[1], CultureInfo.InvariantCulture),
            float.Parse(parts[2], CultureInfo.InvariantCulture),
            float.Parse(parts[3], CultureInfo.InvariantCulture
            )));
    }

    private void ParseFace(string[] parts)
    {
        if (parts.Length < 4)
            throw new ObjParseException("Invalid face definition");

        var vertexIndices = new List<int>();
        var textureIndices = new List<int>();
        var normalIndices = new List<int>();
        bool hasTexture = false;
        bool hasNormal = false;

        for (int i = 1; i < parts.Length; i++)
        {
            var indices = parts[i].Split('/');
            if (indices.Length > 0 && int.TryParse(indices[0], out int vertexIndex))
            {
                vertexIndices.Add(vertexIndex - 1); // OBJ indices are 1-based
            }

            if (indices.Length > 1 && !string.IsNullOrEmpty(indices[1]) &&
                int.TryParse(indices[1], out int textureIndex))
            {
                textureIndices.Add(textureIndex - 1);
                hasTexture = true;
            }

            if (indices.Length > 2 && int.TryParse(indices[2], out int normalIndex))
            {
                normalIndices.Add(normalIndex - 1);
                hasNormal = true;
            }
        }

        var face = new Face
        {
            VertexIndices = vertexIndices.ToArray(),
            TextureCoordinateIndices = hasTexture ? textureIndices.ToArray() : null,
            NormalIndices = hasNormal ? normalIndices.ToArray() : null,
            MaterialName = _currentMaterial // Associate the current material with the face
        };

        _faces.Add(face);

        // Add the face to the current material group if one exists
        if (_currentMaterial != null)
        {
            _materialGroups[_currentMaterial].Add(face);
        }
    }
    
    public IReadOnlyList<Face> GetFacesForMaterial(string materialName)
    {
        return _materialGroups.TryGetValue(materialName, out var faces) 
            ? faces 
            : Array.Empty<Face>();
    }
    
}

public class ObjParseException : Exception
{
    public ObjParseException(string message) : base(message) { }
    public ObjParseException(string message, Exception innerException) : base(message, innerException) { }
}