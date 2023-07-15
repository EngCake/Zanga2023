using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CakeEngineering
{
    [Serializable]
    public class LayerState : ICloneable
    {
        private readonly Dictionary<Vector2, EntityState> _grid;

        public LayerState()
        {
            _grid = new Dictionary<Vector2, EntityState>();
        }

        public EntityState this[Vector2 position]
        {
            get
            {
                return _grid[position];
            }
            set
            {
                if (value != null)
                    _grid[position] = value;
                else
                    _grid.Remove(position);
            }
        }

        public bool HasEntityAt(Vector2 position)
        {
            return _grid.ContainsKey(position);
        }

        public object Clone()
        {
            var clone = new LayerState();
            foreach (var (position, entityState) in _grid)
            {
                clone._grid[position] = (EntityState) entityState.Clone();
            }
            return clone;
        }

        public IEnumerator<EntityState> GetEnumerator()
        {
            foreach (var (_, entityState) in _grid)
                yield return entityState;
        }

        public IEnumerable<EntityState> FindByAttribute(string attribute)
        {
            return _grid.Values.Where(entityState => entityState.HasAttribute(attribute));
        }

        public List<EntityState> GetAdjacent4(Vector2 position)
        {
            var deltas = new List<Vector2> { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            var result = new List<EntityState>();
            foreach (var direction in deltas)
            {
                var nextPosition = position + direction;
                if (_grid.ContainsKey(nextPosition))
                    result.Add(_grid[nextPosition]);
            }
            return result;
        }

        public EntityState FindStateOfEntity(Entity entity)
        {
            return _grid.Values.FirstOrDefault(entityState => entityState.Entity == entity);
        }
    }
}