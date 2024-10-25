namespace System03.Core.ECS;

public readonly struct Entity(int id)
{
    public int Id { get; } = id;
}