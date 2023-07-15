using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CakeEngineering
{
    [Serializable]
    public class EntityState : IEnumerable<EntityAttribute>, ICloneable
    {
        private readonly List<EntityAttribute> _attributes;

        private readonly Entity _entity;

        private readonly Vector2 _position;

        private readonly Layer _layer;

        public Vector2 Position => _position;

        public Entity Entity => _entity;

        public List<EntityAttribute> Attributes => _attributes;

        public Layer Layer => _layer;

        public EntityState(Entity entity, List<EntityAttribute> attributes, Vector2 position, Layer layer)
        {
            _entity = entity;
            _attributes = attributes;
            _position = position;
            _layer = layer;
        }

        public bool HasAttribute(string attributeName)
        {
            return _attributes.Any(attribute => attribute.Name == attributeName);
        }

        public IEnumerator<EntityAttribute> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public object Clone()
        {
            var attributes = new List<EntityAttribute>();
            var clone = new EntityState(_entity, attributes, _position, _layer);
            _attributes.ForEach(attribute => clone._attributes.Add(attribute));
            return clone;
        }
        
        public EntityState WithNewPosition(Vector2 position)
        {
            return new EntityState(_entity, _attributes, position, _layer);
        }

        public EntityState WithAttribute(EntityAttribute attribute)
        {
            if (_attributes.Contains(attribute))
                return null;
            var attributes = new List<EntityAttribute>();
            _attributes.ForEach(attribute => attributes.Add(attribute));
            attributes.Add(attribute);
            return new EntityState(_entity, attributes, _position, _layer);
        }

        public EntityState WithoutAttribute(EntityAttribute attribute)
        {
            var attributes = new List<EntityAttribute>();
            foreach (var attr in _attributes)
                if (attr.Name != attribute.Name)
                    attributes.Add(attr);
            return new EntityState(_entity, attributes, _position, _layer);
        }
    }

    [Serializable]
    public enum EntityType
    {
        Human,
        Animal,
        Object
    }
}