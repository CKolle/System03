using System;
using System.IO;
using System03.Core;

namespace System03.Tests.Core;

public class ConfigurationTests
{
    [Fact]
    public void Constructor_DefaultValues_AreCorrect()
    {
        // Arrange and Act
        var config = new Configuration();
        
        // Assert
        Assert.Equal(60, config.TargetFPS);
        Assert.Equal(1920, config.DefaultResolutionWidth);
        Assert.Equal(1080, config.DefaultResolutionHeight);
        Assert.True(config.VSync);
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void SetTargetFPS_InvalidValue_ThrowsException(int targetFPS)
    {
        // Arrange
        var config = new Configuration();
        
        // Act & Assert
        Assert.Throws<ArgumentException>(() => config.TargetFPS = targetFPS);
    }
    
    [Fact]
    public void LoadFromFile_InvalidFile_ThrowsException()
    {
        // Arrange
        var tempPath = Path.GetTempFileName();
        File.Delete(tempPath);
        
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => Configuration.LoadFromFile(tempPath));
    }
    
    [Fact]
    public void SaveToFile_ValidConfig_SavesAndLoads()
    {
        var config = new Configuration
        {
            TargetFPS = 120,
            DefaultResolutionWidth = 1280,
            DefaultResolutionHeight = 720,
            VSync = true,
        };
        var tempPath = Path.GetTempFileName();

        try
        {
            // Act
            config.SaveToFile(tempPath);
            var loadedConfig = Configuration.LoadFromFile(tempPath);

            // Assert
            Assert.Equal(config.TargetFPS, loadedConfig.TargetFPS);
            Assert.Equal(config.DefaultResolutionWidth, loadedConfig.DefaultResolutionWidth);
            Assert.Equal(config.DefaultResolutionHeight, loadedConfig.DefaultResolutionHeight);
            Assert.Equal(config.VSync, loadedConfig.VSync);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
    
    [Fact]
    public void SaveToFile_OverrideExistingFile_SavesAndLoads()
    {
        var config = new Configuration
        {
            TargetFPS = 120,
            DefaultResolutionWidth = 1280,
            DefaultResolutionHeight = 720,
            VSync = true,
        };
        var tempPath = Path.GetTempFileName();

        try
        {
            // Act
            config.SaveToFile(tempPath);
            config.TargetFPS = 144;
            config.SaveToFile(tempPath);
            var loadedConfig = Configuration.LoadFromFile(tempPath);

            // Assert
            Assert.Equal(config.TargetFPS, loadedConfig.TargetFPS);
            Assert.Equal(config.DefaultResolutionWidth, loadedConfig.DefaultResolutionWidth);
            Assert.Equal(config.DefaultResolutionHeight, loadedConfig.DefaultResolutionHeight);
            Assert.Equal(config.VSync, loadedConfig.VSync);
        }
        finally
        {
            File.Delete(tempPath);
        }
    }
}