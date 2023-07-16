using System.Linq;
using UnityEngine;

namespace CakeEngineering
{
    public class PlayerMovementSystem : EntityAttributeSystem
    {
        private GridState _currentState;

        private GridState _nextState;

        public override void Process(Vector2 playerMovement)
        {
            _currentState = (GridState)_gameManager.CurrentGridState.Clone();
            _nextState = _gameManager.CurrentGridState;
            var playerPosition = _currentState.PlayerState.Position;
            if (CanMoveEntity(playerPosition, playerMovement))
                MoveEntity(playerPosition, playerMovement);
        }

        public bool CanMoveEntity(Vector2 entityPosition, Vector2 movement, bool isPushed = false)
        {
            var currentEntity = _currentState[entityPosition];
            if (currentEntity.HasAttribute("Breakable") && isPushed)
            {
                return true;
            }
            else if (currentEntity.HasAttribute("Player") || currentEntity.HasAttribute("Movable"))
            {
                var nextPosition = entityPosition + movement;
                if (!_currentState.HasEntityAt(nextPosition))
                    return true;
                return CanMoveEntity(nextPosition, movement, true);
            }
            else if (currentEntity.HasAttribute("Portal A") || currentEntity.HasAttribute("Portal B"))
            {
                var otherPortalPosition = FindOtherPortalPosition(currentEntity);
                Vector2 nextPosition = otherPortalPosition + movement;
                if (!_currentState.HasEntityAt(nextPosition))
                    return true;
                return CanMoveEntity(nextPosition, movement);
            }
            return false;
        }

        public void MoveEntity(Vector2 entityPosition, Vector2 movement)
        {
            var previousEntity = (EntityState)null;
            var currentPosition = entityPosition;
            var isPushed = false;
            while (_currentState.HasEntityAt(currentPosition))
            {
                var current = _nextState[currentPosition];
                if ( isPushed && current.HasAttribute("Breakable") && !current.HasAttribute("Movable") || isPushed && current.HasAttribute("Breakable") && !CanMoveEntity(currentPosition, movement) )
                {
                    _nextState[currentPosition] = previousEntity;
                    previousEntity = null;
                    break;
                }
                else if ( current.HasAttribute("Movable") || !current.HasAttribute("Portal A") && !current.HasAttribute("Portal B") )
                {
                    _nextState[currentPosition] = previousEntity;
                    currentPosition += movement;
                    previousEntity = current.WithNewPosition(currentPosition);
                }
                else
                {
                    currentPosition = FindOtherPortalPosition(current) + movement;
                    previousEntity = previousEntity.WithNewPosition(currentPosition);
                }
                isPushed = true;
            }
            if (previousEntity != null)
                _nextState[currentPosition] = previousEntity;
        }

        private Vector2 FindOtherPortalPosition(EntityState portal)
        {
            return portal.HasAttribute("Portal A") ?
                _currentState.FindByAttribute("Portal B").First().Position :
                _currentState.FindByAttribute("Portal A").First().Position;
        }
    }
}