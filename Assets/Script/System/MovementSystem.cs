using System.Linq;
using UnityEngine;

namespace CakeEngineering
{
    public class 
        MovementSystem : EntitySystem
    {
        private LevelState _currentState;

        private LevelState _nextState;

        public override void Process()
        {
            _currentState = (LevelState)_gameManager.CurrentGridState.Clone();
            _nextState = _gameManager.CurrentGridState;
            MovePlayer();
            MoveHorizontallyMovingEntities();
            MoveVerticallyMovingEntities();
        }

        private void MoveVerticallyMovingEntities()
        {
            var verticalEntitiesStates = _currentState.FindByAttribute("Moving Vertically");
            foreach (var verticalEntityState in verticalEntitiesStates)
            {
                if (verticalEntityState.HasAttribute("Player"))
                    continue;
                var currentEntityPosition = verticalEntityState.Position;
                var currentEntityVelocity = verticalEntityState.Velocity;
                if (CanMoveEntity(currentEntityPosition, currentEntityVelocity))
                    MoveEntity(currentEntityPosition, currentEntityVelocity);
                else
                    _nextState[currentEntityPosition] = verticalEntityState.WithVelocity(-currentEntityVelocity);
            }
        }

        private void MoveHorizontallyMovingEntities()
        {
            var horizontalEntitiesStates = _currentState.FindByAttribute("Moving Horizontally");
            foreach (var horizontalEntityState in horizontalEntitiesStates)
            {
                if (horizontalEntityState.HasAttribute("Player"))
                    continue;
                var currentEntityPosition = horizontalEntityState.Position;
                var currentEntityVelocity = horizontalEntityState.Velocity;
                if (CanMoveEntity(currentEntityPosition, currentEntityVelocity))
                    MoveEntity(currentEntityPosition, currentEntityVelocity);
                else
                {
                    _nextState[currentEntityPosition] = horizontalEntityState.WithVelocity(-currentEntityVelocity);
                    if (CanMoveEntity(currentEntityPosition, -currentEntityVelocity))
                        MoveEntity(currentEntityPosition, -currentEntityVelocity);
                }
                _currentState = (LevelState)_nextState.Clone();
            }
        }

        private void MovePlayer()
        {
            var playerState = _currentState.PlayerState;
            if (CanMoveEntity(playerState.Position, playerState.Velocity))
            {
                MoveEntity(playerState.Position, playerState.Velocity);
                _currentState = (LevelState)_nextState.Clone();
            }
            else
            {
                var playerVelocty = playerState.Velocity;
                var playerVelocityRotated = new Vector2(playerVelocty.y, -playerVelocty.x);
                ShakeEntity(_currentState.PlayerState.Position, playerVelocityRotated);
            }
        }

        private void ShakeEntity(Vector2 entityPosition, Vector2 direction)
        {
            var gameObject = _nextState[entityPosition].Entity.gameObject;
            var delta = direction * 0.4f;
            LeanTween.moveLocal(gameObject, entityPosition + delta, 0.2f / 8.0f)
                .setOnComplete(_ => LeanTween.moveLocal(gameObject, entityPosition, 0.2f / 8.0f))
                .setOnComplete(_ => LeanTween.moveLocal(gameObject, entityPosition -delta, 0.2f / 8.0f))
                .setOnComplete(_ => LeanTween.moveLocal(gameObject, entityPosition, 0.2f / 8.0f))
                .setRepeat(2);
        }

        public bool CanMoveEntity(Vector2 entityPosition, Vector2 movement, bool isPushed = false, bool previousEntityIsPlayer = false)
        {
            var currentEntityState = _currentState[entityPosition];
            if (currentEntityState.HasAttribute("Breakable") && isPushed || currentEntityState.HasAttribute("Win") && previousEntityIsPlayer)
                return true;
            else if (!isPushed && CanMoveByItself(currentEntityState) || isPushed && currentEntityState.HasAttribute("Pushable"))
            {
                var nextPosition = entityPosition + movement;
                if (!_currentState.HasEntityAt(nextPosition))
                    return true;
                return CanMoveEntity(nextPosition, movement, true, currentEntityState.HasAttribute("Player"));
            }
            else if (currentEntityState.HasAttribute("Portal A") || currentEntityState.HasAttribute("Portal B"))
            {
                var otherPortalPosition = FindOtherPortalPosition(currentEntityState);
                Vector2 nextPosition = otherPortalPosition + movement;
                if (!_currentState.HasEntityAt(nextPosition))
                    return true;
                return CanMoveEntity(nextPosition, movement, true, currentEntityState.HasAttribute("Player"));
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
                var currentEntity = _nextState[currentPosition];
                var breakableUnpushable = isPushed && currentEntity.HasAttribute("Breakable") && !currentEntity.HasAttribute("Pushable");
                var breakableUnmovable = isPushed && currentEntity.HasAttribute("Breakable") && !CanMoveEntity(currentPosition, movement);
                if (breakableUnmovable || breakableUnpushable)
                {
                    _nextState[currentPosition] = previousEntity;
                    previousEntity = null;
                    break;
                }
                else if (currentEntity.HasAttribute("Win") && previousEntity.HasAttribute("Player"))
                {
                    _nextState[currentPosition] = previousEntity;
                    previousEntity = null;
                    _gameManager.Win();
                    break;
                }
                else if ( currentEntity.HasAttribute("Pushable") || !currentEntity.HasAttribute("Portal A") && !currentEntity.HasAttribute("Portal B") )
                {
                    _nextState[currentPosition] = previousEntity;
                    currentPosition += movement;
                    previousEntity = currentEntity.WithNewPosition(currentPosition);
                }
                else
                {
                    currentPosition = FindOtherPortalPosition(currentEntity) + movement;
                    previousEntity = previousEntity.WithNewPosition(currentPosition);
                }
                isPushed = true;
            }
            if (previousEntity != null)
                _nextState[currentPosition] = previousEntity;
        }

        private bool CanMoveByItself(EntityState entityState)
        {
            return entityState.HasAttribute("Player") || entityState.HasAttribute("Moving Horizontally") || entityState.HasAttribute("Moving Vertically");
        }

        private Vector2 FindOtherPortalPosition(EntityState portal)
        {
            return portal.HasAttribute("Portal A") ?
                _currentState.FindByAttribute("Portal B").First().Position :
                _currentState.FindByAttribute("Portal A").First().Position;
        }
    }
}