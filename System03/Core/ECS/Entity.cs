namespace System03.Core.ECS;

public struct Entity
{
    public int Id { get; }
    
    public Entity(int id)
    {
        Id = id;
    }
}