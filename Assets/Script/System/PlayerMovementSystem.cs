using UnityEngine;

namespace CakeEngineering
{
    public class PlayerMovementSystem : EntityAttributeSystem
    {
        private LayerState _currentState;

        private LayerState _nextState;

        public override void Process(Vector2 playerMovement)
        {
            _currentState = (LayerState)_gameManager.NextGridState.Clone();
            _nextState = _gameManager.NextGridState;
            var playerPosition = _currentState.PlayerState.Position;
            if (CanMoveEntity(playerPosition, playerMovement))
                MoveEntity(playerPosition, playerMovement);
        }

        private bool CanMoveEntity(Vector2 entityPosition, Vector2 movement)
        {
            var nextPosition = entityPosition + movement;
            if (!_currentState.HasEntityAt(nextPosition))
                return true;
            var collidingEntity = _currentState[nextPosition];
            var isCollidingEntityMovable = collidingEntity.HasAttribute("Movable");
            if (isCollidingEntityMovable)
                return CanMoveEntity(nextPosition, movement);
            return false;
        }

        private void MoveEntity(Vector2 entityPosition, Vector2 movement)
        {
            var previousEntity = (EntityState)null;
            var currentPosition = entityPosition;
            while (_currentState.HasEntityAt(currentPosition))
            {
                var temp = _nextState[currentPosition];
                _nextState[currentPosition] = previousEntity;
                previousEntity = temp.WithNewPosition(currentPosition + movement);
                currentPosition += movement;
            }
            _nextState[currentPosition] = previousEntity;
        }
    }
}