using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CakeEngineering
{
    [Serializable]
    public enum Layer
    {
        Ground,
        Objects,
        Sky
    }

    public class GridState : ICloneable
    {
        private readonly LayerState[] _layersStates;

        private GridState()
        {
            _layersStates = new LayerState[3];
            for (var i = 0; i < 3; i++)
                _layersStates[i] = new LayerState();
        }

        public static GridState InitializeFromEntities(Entity[] entities)
        {
            var gridState = new GridState();
            foreach (var entity in entities)
                gridState._layersStates[(int)entity.Layer][entity.Position] = entity.InitialState;
            return gridState;
        }

        public LayerState this[Layer layer]
        {
            get
            {
                return _layersStates[(int)layer];
            }
        }

        public EntityState PlayerState
        {
            get
            {
                foreach (var layerState in _layersStates)
                {
                    var player = layerState.FindByAttribute("Player").FirstOrDefault();
                    if (player != null)
                        return player;
                }
                return null;
            }
        }

        public EntityState FindStateOfEntity(Entity entity)
        {
            foreach (var layerState in _layersStates)
            {
                var entityState = layerState.FindStateOfEntity(entity);
                if (entityState != null) return entityState;
            }
            return null;
        }

        public List<EntityState> StatesOfEntitesAt(Vector2 position)
        {
            var entities = new List<EntityState>();
            foreach (var layerState in _layersStates)
                if (layerState.HasEntityAt(position))
                    entities.Add(layerState[position]);
            return entities;
        }

        public object Clone()
        {
            var clone = new GridState();
            for (var i = 0; i < 3; i++)
                clone._layersStates[i] = (LayerState) _layersStates[i].Clone();
            return clone;
        }
    }
}