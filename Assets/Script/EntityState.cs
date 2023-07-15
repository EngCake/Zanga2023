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
        [SerializeField]
        private bool _isActive;

        [SerializeField]
        private EntityType _type;

        [SerializeField]
        private string _name;

        [SerializeField]
        private List<EntityAttribute> _attributes;

        [SerializeField]
        private Entity _entity;

        public bool Active => _isActive;

        public string Name => _name;

        public EntityType Type => _type;

        private EntityState(Entity entity)
        {
            _isActive = true;
            _attributes = new List<EntityAttribute>();
            _entity = entity;
        }

        public EntityState(Entity entity, List<EntityAttribute> attributes)
        {
            _isActive = true;
            _attributes = attributes;
            _entity = entity;
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
            var clone = new EntityState(_entity);
            clone._isActive = _isActive;
            _attributes.ForEach(attribute => clone._attributes.Add(attribute));
            return clone;
        }

        public void MoveEntity(Vector2 position)
        {
            if (position != _entity.Position)
                _entity.SetPosition(position);
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