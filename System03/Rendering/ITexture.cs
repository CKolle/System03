namespace System03.Rendering;

public interface ITexture
{
    // Unit is the texture unit to bind the texture to (0-31) which are the texture units available in OpenGL
    // Don't know about vulkan or other APIs need to investigate
    void Bind(int unit);
    void Unbind();
}