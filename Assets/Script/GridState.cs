using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CakeEngineering
{
    public struct GridStateItem
    {
        private Vector2 _position;

        private EntityState _state;

        public GridStateItem(Vector2 Position, EntityState State)
        {
            _position = Position;
            _state = State;
        }

        public void Deconstruct(out Vector2 position, out EntityState state)
        {
            position = _position;
            state = _state;
        }
    }

    [Serializable]
    public class GridState : ICloneable, IEnumerable<GridStateItem>
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

        public void MoveAllEntities()
        {
            foreach (var (position, entity) in _grid)
                entity.MoveEntity(position);
        }

        public IEnumerator<GridStateItem> GetEnumerator()
        {
            foreach (var (position, entity) in _grid)
                yield return new GridStateItem(position, entity);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}