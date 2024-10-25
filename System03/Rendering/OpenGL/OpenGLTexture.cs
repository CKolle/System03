using System.Runtime.InteropServices;
using OpenTK.Graphics.ES30;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using PixelFormat = OpenTK.Graphics.ES30.PixelFormat;
using PixelType = OpenTK.Graphics.ES30.PixelType;
using TextureMagFilter = OpenTK.Graphics.ES30.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.ES30.TextureMinFilter;
using TextureParameterName = OpenTK.Graphics.ES30.TextureParameterName;
using TextureTarget = OpenTK.Graphics.ES30.TextureTarget;
using TextureTarget2d = OpenTK.Graphics.ES30.TextureTarget2d;
using TextureWrapMode = OpenTK.Graphics.ES30.TextureWrapMode;

namespace System03.Rendering.OpenGL;

public class OpenGLTexture : ITexture, IDisposable
{
    private readonly int _textureId;
    private readonly int _width;
    private readonly int _height;
    private bool _disposed;

    public OpenGLTexture(string path)
    {
        using var image = Image.Load<Rgba32>(path);
        image.Mutate(x => x.Flip(FlipMode.Vertical));
            
        _width = image.Width;
        _height = image.Height;

        // Create continuous memory block
        var pixels = new byte[4 * _width * _height];
        
        // Copy pixel data
        image.CopyPixelDataTo(pixels);

        // Generate and bind the texture
        _textureId = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, _textureId);
        CheckGLError("Texture generation");

        // Ensure pixel data is pinned and won't be moved by GC
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            var ptr = handle.AddrOfPinnedObject();
            GL.TexImage2D(
                TextureTarget2d.Texture2D,
                0,                          // mipmap level
                TextureComponentCount.Rgba, // internal format
                _width,
                _height,
                0,                          // border
                PixelFormat.Rgba,           // format
                PixelType.UnsignedByte,     // type
                ptr                         // pixels
            );
            CheckGLError("TexImage2D");
        }
        finally
        {
            handle.Free();
        }

        // Set texture parameters
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        CheckGLError("Texture parameters");

        // Generate mipmaps
        GL.GenerateMipmap(TextureTarget.Texture2D);
        CheckGLError("Mipmap generation");

        // Unbind the texture
        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    public void Bind(int unit)
    {
        if (unit < 0 || unit >= GL.GetInteger(GetPName.MaxCombinedTextureImageUnits))
        {
            throw new ArgumentException($"Invalid texture unit: {unit}");
        }

        GL.ActiveTexture(TextureUnit.Texture0 + unit);
        CheckGLError($"ActiveTexture {unit}");
        
        GL.BindTexture(TextureTarget.Texture2D, _textureId);
        CheckGLError("BindTexture");
    }

    public void Unbind()
    {
        GL.BindTexture(TextureTarget.Texture2D, 0);
        CheckGLError("Unbind texture");
    }

    private static void CheckGLError(string action)
    {
        var error = GL.GetError();
        if (error != ErrorCode.NoError)
        {
            throw new Exception($"OpenGL Error during {action}: {error}");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing && _textureId != 0)
            {
                GL.DeleteTexture(_textureId);
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~OpenGLTexture()
    {
        Dispose(false);
    }
}