using System.Runtime.CompilerServices;

namespace System03.Core.ECS.Component;

public class ComponentManager
{
    // TODO - This is bad, the components are not stored sequentially in memory, but I don't care right now.
    // TODO - I have some ideas on how to improve this, but I will leave it for later.
    private readonly Dictionary<Type, Dictionary<int, IComponent>> _components = new();
    private readonly Dictionary<int, HashSet<Type>> _entityComponents = new();
    private readonly Dictionary<Type, List<Entity>> _entityQueryCache = new();
    private bool _cacheIsDirty = true;

    // New method for struct components
    public void AddComponent<T>(Entity entity, T component) where T : struct, IComponent
    {
        var type = typeof(T);
        if (!_components.ContainsKey(type))
            _components[type] = new Dictionary<int, IComponent>();
        
        // Box the struct into an IComponent
        _components[type][entity.Id] = component;

        if (!_entityComponents.ContainsKey(entity.Id))
            _entityComponents[entity.Id] = [];
        
        _entityComponents[entity.Id].Add(type);
        _cacheIsDirty = true;
    }
    
    public IReadOnlyList<Entity> GetEntitiesWithComponent<T>() where T : struct, IComponent
    {
        var type = typeof(T);
    
        // If cache is valid and exists, return it
        if (!_cacheIsDirty && _entityQueryCache.TryGetValue(type, out var cachedEntities))
            return cachedEntities;

        // Get or create the list
        if (!_entityQueryCache.TryGetValue(type, out var entityList))
        {
            entityList = new List<Entity>(); 
            _entityQueryCache[type] = entityList;
        }
        else
            entityList.Clear();

        // Populate the list
        foreach (var kvp in _entityComponents)
        {
            if (kvp.Value.Contains(type))
                entityList.Add(new Entity(kvp.Key));  
        }
    
        _cacheIsDirty = false;
        return entityList;
    }
    
    public ref T GetComponent<T>(Entity entity) where T : struct, IComponent
    {
        var type = typeof(T);
        if (!_components.TryGetValue(type, out var dictObj))
            throw new ArgumentException($"No components of type {type} found");
            
        if (!dictObj.TryGetValue(entity.Id, out var component))
            throw new ArgumentException($"Entity {entity.Id} does not have component of type {type}");
            
        // Unbox the IComponent into a struct 😎 really unsafe yay!
        return ref Unsafe.Unbox<T>(component);
    }
    
    // This should avoid copying the struct
    public delegate void ComponentAction<T>(Entity entity, ref T component) where T : struct, IComponent;
    public void ForEachComponent<T>(ComponentAction<T> action) where T : struct, IComponent
    {
        var entities = GetEntitiesWithComponent<T>();
        foreach (var entity in entities)
        {
            ref var component = ref GetComponent<T>(entity);
            action(entity, ref component);
        }
    }
    
    public IEnumerable<T> GetAllComponentsOfType<T>() where T : IComponent
    {
        var type = typeof(T);
        if (!_components.TryGetValue(type, out var entityComponents))
            return Enumerable.Empty<T>();

        if (typeof(T).IsValueType)
            return entityComponents.Values.Select(c => (T)c);

        return entityComponents.Values.Select(c => (T)c).Where(c => c != null);
    }

}