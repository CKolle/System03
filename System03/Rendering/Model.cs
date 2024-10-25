using OpenTK.Mathematics;

namespace System03.Rendering;

// TODO - Investigate maybe grouping the submeshes by material will result in less draw calls
// As we can have one draw call per material instead of one draw call per submesh
public record struct Submesh
{
    public required int StartIndex { get; init; }
    public required int IndexCount { get; init; }
    public required Material Material { get; init; }
}

public class Model
{
    public IMesh Mesh { get; }
    public IReadOnlyList<Submesh> Submeshes { get; }
    
    // Transform properties
    private Vector3 _position;
    private Quaternion _rotation = Quaternion.Identity;
    private Vector3 _scale = Vector3.One;
    private Matrix4 _modelMatrix = Matrix4.Identity;
    private bool _transformDirty = true;

    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            _transformDirty = true;
        }
    }

    public Quaternion Rotation
    {
        get => _rotation;
        set
        {
            _rotation = value;
            _transformDirty = true;
        }
    }

    public Vector3 Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            _transformDirty = true;
        }
    }

    public Model(IMesh mesh, IReadOnlyList<Submesh> submeshes)
    {
        Mesh = mesh;
        Submeshes = submeshes;
    }

    public Matrix4 GetModelMatrix()
    {
        if (_transformDirty)
        {
            _modelMatrix = Matrix4.CreateScale(_scale) *
                           Matrix4.CreateFromQuaternion(_rotation) *
                           Matrix4.CreateTranslation(_position);
            _transformDirty = false;
        }
        return _modelMatrix;
    }
}