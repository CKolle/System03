using System.IO;
using System.Linq;
using System03.FileFormats;

namespace System03.Tests.FileFormats;

public class ObjFileTest
{
    private static Stream GenerateStreamFromString(string content)
    {
        return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
    }
    
    [Fact]
    public void ParseVertex_CorrectlyParsesValidVertex()
    {
        // Arrange
        const string objContent = "v 1.000000 -1.000000 2.000000";
        using var stream = GenerateStreamFromString(objContent);

        // Act 
        var objFile = new ObjFile(stream);

        // Assert
        Assert.Single(objFile.GeometricVertices);
        var vertex = objFile.GeometricVertices[0];
        Assert.Equal(1.0f, vertex.X);
        Assert.Equal(-1.0f, vertex.Y);
        Assert.Equal(2.0f, vertex.Z);
    }
}