namespace System03.Core.ECS.Component
{
    public class ComponentManager
    {
        // TODO - This is bad, the components are not stored sequentially in memory, but I don't care right now.
        private readonly Dictionary<Type, Dictionary<int, IComponent>> _components = new();
        private readonly Dictionary<int, HashSet<Type>> _entityComponents = new();

        public void AddComponent<T>(Entity entity, T component) where T : class, IComponent
        {
            var type = typeof(T);
            if (!_components.ContainsKey(type))
                _components[type] = new Dictionary<int, IComponent>();
            
            _components[type][entity.Id] = component;

            // Track which components an entity has
            if (!_entityComponents.ContainsKey(entity.Id))
                _entityComponents[entity.Id] = [];
            
            _entityComponents[entity.Id].Add(type);
        }

        public T? GetComponent<T>(Entity entity) where T : class, IComponent
        {
            var type = typeof(T);
            if (_components.TryGetValue(type, out var entityComponents) && 
                entityComponents.TryGetValue(entity.Id, out var component))
                return component as T;
            return null;
        }

        public bool HasComponent<T>(Entity entity) where T : IComponent
        {
            return _entityComponents.TryGetValue(entity.Id, out var components) && 
                   components.Contains(typeof(T));
        }

        public bool HasComponents(Entity entity, params Type[] componentTypes)
        {
            if (!_entityComponents.TryGetValue(entity.Id, out var components))
                return false;

            return componentTypes.All(type => components.Contains(type));
        }

        public void RemoveComponent<T>(Entity entity) where T : IComponent
        {
            var type = typeof(T);
            if (!_components.TryGetValue(type, out var entityComponents)) return;
            entityComponents.Remove(entity.Id);
            _entityComponents[entity.Id].Remove(type);

            if (entityComponents.Count == 0)
                _components.Remove(type);
        }

        public void RemoveAllComponents(Entity entity)
        {
            if (!_entityComponents.TryGetValue(entity.Id, out var componentTypes)) return;
            foreach (var type in componentTypes.ToList())
            {
                if (!_components.TryGetValue(type, out var entityComponents)) continue;
                entityComponents.Remove(entity.Id);
                        
                if (entityComponents.Count == 0)
                    _components.Remove(type);
            }
            _entityComponents.Remove(entity.Id);
        }

        public IEnumerable<Entity> GetEntitiesWithComponent<T>() where T : IComponent
        {
            var type = typeof(T);
            return _components.TryGetValue(type, out var entityComponents) 
                ? entityComponents.Keys.Select(id => new Entity(id)) : Enumerable.Empty<Entity>();
        }

        public IEnumerable<Entity> GetEntitiesWithComponents(params Type[] componentTypes)
        {
            return _entityComponents
                .Where(kvp => componentTypes.All(type => kvp.Value.Contains(type)))
                .Select(kvp => new Entity(kvp.Key));
        }

        public IEnumerable<T?> GetAllComponentsOfType<T>() where T : class, IComponent
        {
            var type = typeof(T);
            return _components.TryGetValue(type, out var entityComponents)
                ? entityComponents.Values.Select(c => c as T).Where(c => c != null) : Enumerable.Empty<T>();
        }
    }
}