using OpenTK.Mathematics;
using System03.FileFormats;

namespace System03.Rendering;

public class Material
{
    public Vector3 DiffuseColor { get; set; }
    public Vector3 SpecularColor { get; set; }
    public float Shininess { get; set; }
    public ITexture? DiffuseTexture { get; set; }

    public static Material FromMtlMaterial(MtlFile.Material mtl, ITexture? diffuseTexture)
    {
        return new Material
        {
            DiffuseColor = new Vector3(mtl.Diffuse.R, mtl.Diffuse.G, mtl.Diffuse.B),
            SpecularColor = new Vector3(mtl.Specular.R, mtl.Specular.G, mtl.Specular.B),
            Shininess = mtl.Shininess,
            DiffuseTexture = diffuseTexture
        };
    }
}