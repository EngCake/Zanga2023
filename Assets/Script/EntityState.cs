using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace CakeEngineering
{
    [Serializable]
    public class EntityState : IEnumerable<EntityAttribute>, ICloneable
    {
        private readonly bool _isActive;

        private readonly EntityType _type;

        private readonly string _name;

        private readonly List<EntityAttribute> _attributes;

        private readonly Entity _entity;

        private readonly Vector2 _position;

        public bool Active => _isActive;

        public string Name => _name;

        public EntityType Type => _type;

        public Vector2 Position => _position;

        public Entity Entity => _entity;

        private EntityState(bool isActive, EntityType type, string name, List<EntityAttribute> attributes, Entity entity, Vector2 position)
        {
            _isActive = isActive;
            _type = type;
            _name = name;
            _attributes = attributes;
            _entity = entity;
            _position = position;
        }

        public EntityState(Entity entity, List<EntityAttribute> attributes, bool isActive = true)
        {
            _isActive = isActive;
            _attributes = attributes;
            _entity = entity;
            _position = entity.Position;
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
            var clone = new EntityState(_isActive, _type, _name, attributes, _entity, _position);
            _attributes.ForEach(attribute => clone._attributes.Add(attribute));
            return clone;
        }

        public void UpdateEntity()
        {
            _entity.SetPosition(_position);
        }
        
        public EntityState WithNewPosition(Vector2 position)
        {
            return new EntityState(_isActive, _type, _name, _attributes, _entity, position);
        }

        public EntityState WithAttribute(EntityAttribute attribute)
        {
            var attributes = new List<EntityAttribute>();
            _attributes.ForEach(attribute => attributes.Add(attribute));
            attributes.Add(attribute);
            return new EntityState(_isActive, _type, _name, attributes, _entity, _position);
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