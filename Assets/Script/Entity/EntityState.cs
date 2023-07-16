using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CakeEngineering
{
    [Serializable]
    public class EntityState : ICloneable
    {
        private readonly List<EntityAttribute> _attributes;

        private readonly Entity _entity;

        private readonly Vector2 _position;

        private readonly Vector2 _velocity;

        public List<EntityAttribute> Attributes => _attributes;

        public Entity Entity => _entity;

        public Vector2 Position => _position;

        public Vector2 Velocity => _velocity;

        public EntityState(List<EntityAttribute> attributes, Entity entity, Vector2 position, Vector2 velocity)
        {
            _attributes = attributes;
            _entity = entity;
            _position = position;
            _velocity = velocity;
        }

        public bool HasAttribute(string attributeName)
        {
            return _attributes.Any(attribute => attribute.Name == attributeName);
        }

        public object Clone()
        {
            var attributes = new List<EntityAttribute>();
            var clone = new EntityState(attributes, _entity, _position, _velocity);
            _attributes.ForEach(attribute => clone._attributes.Add(attribute));
            return clone;
        }
        
        public EntityState WithNewPosition(Vector2 position)
        {
            return new EntityState(_attributes, _entity, position, _velocity);
        }

        public EntityState WithAttribute(EntityAttribute attribute)
        {
            if (_attributes.Contains(attribute))
                return null;
            var attributes = new List<EntityAttribute>();
            _attributes.ForEach(attribute => attributes.Add(attribute));
            attributes.Add(attribute);
            return new EntityState(attributes, _entity, _position, _velocity);
        }

        public EntityState WithoutAttribute(EntityAttribute attribute)
        {
            var attributes = new List<EntityAttribute>();
            foreach (var attr in _attributes)
                if (attr.Name != attribute.Name)
                    attributes.Add(attr);
            return new EntityState(attributes, _entity, _position, _velocity);
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