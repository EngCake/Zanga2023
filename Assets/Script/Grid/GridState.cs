using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CakeEngineering
{
    [Serializable]
    public class GridState : ICloneable
    {
        private readonly Dictionary<Vector2, EntityState> _grid;

        private GridState()
        {
            _grid = new Dictionary<Vector2, EntityState>();
        }

        public GridState(Entity[] entities)
        {
            _grid = new Dictionary<Vector2, EntityState>();
            foreach (var entity in entities)
            {
                _grid[entity.Position] = entity.InitialState;
            }
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
            var clone = new GridState();
            foreach (var (position, entityState) in _grid)
            {
                clone._grid[position] = (EntityState) entityState.Clone();
            }
            return clone;
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

        public EntityState FindState(Entity entity)
        {
            return _grid.Values.FirstOrDefault(entityState => entityState.Entity == entity);
        }

        public EntityState PlayerState => _grid.Values.First(entityState => entityState.HasAttribute("Player"));
    }
}