namespace System03.Windowing.Abstractions;

public record WindowConfig(
    int Width = 800,
    int Height = 600,
    bool IsEmbedded = false);