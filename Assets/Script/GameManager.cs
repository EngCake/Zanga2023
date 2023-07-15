using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CakeEngineering
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private List<GridState> _gridTimeline;

        [SerializeField]
        private int _timeIndex = 0;

        [SerializeField]
        private List<EntityAttributeSystem> _systems;

        private PlayerInput _playerInput;

        private InputAction _move;

        private InputAction _undo;

        private bool _lock;

        private void Start()
        {
            _lock = false;

            _playerInput = new PlayerInput();
            _move = _playerInput.Player.Move;
            _move.Enable();
            _undo = _playerInput.Player.Undo;
            _undo.Enable();

            _gridTimeline = new List<GridState>();
            var initialState = new GridState(GetComponentsInChildren<Entity>());
            _gridTimeline.Add(initialState);
        }

        private void Lock()
        {
            _lock = true;
            Invoke(nameof(Unlock), 0.2f);
        }

        private void Unlock()
        {
            _lock = false;
        }

        private void Update()
        {
            var moveDirection = _move.ReadValue<Vector2>();
            if (!_lock && moveDirection != Vector2.zero && (moveDirection.x == 0 || moveDirection.y == 0))
            {
                _gridTimeline.Add((GridState) _gridTimeline.Last().Clone());
                MovePlayer(moveDirection);
                foreach (var system in _systems)
                    system.Process();
                UpdateAllEntities();
                Lock();
                _timeIndex++;
            } else if (!_lock && _undo.IsPressed() && _gridTimeline.Count > 1)
            {
                _gridTimeline.RemoveAt(_gridTimeline.Count - 1);
                _gridTimeline.Last().UpdateAllEntities();
                Lock();
                _timeIndex--;
            }
        }

        private void UpdateAllEntities()
        {
            NextGridState.UpdateAllEntities();
            foreach (var entity in CurrentGridState)
            {
                if (!NextGridState.Any(entityState => entityState.Entity == entity.Entity))
                {
                    entity.Entity.Hide();
                }
            }
            foreach (var entity in NextGridState)
            {
                if (!CurrentGridState.Any(entityState => entityState.Entity == entity.Entity))
                {
                    entity.Entity.Show();
                }
            }
        }

        public void MovePlayer(Vector2 playerMovement)
        {
            foreach (var entityState in _gridTimeline[_timeIndex])
            {
                if (entityState.HasAttribute("Player"))
                {
                    if (CanMoveEntity(entityState.Position, playerMovement))
                        MoveEntity(entityState.Position, playerMovement);
                    return;
                }
            }
        }

        public bool CanMoveEntity(Vector2 entityPosition, Vector2 movement)
        {
            var nextPosition = entityPosition + movement;
            if (!_gridTimeline[_timeIndex].HasEntityAt(nextPosition))
                return true;
            var collidingEntity = _gridTimeline[_timeIndex][nextPosition];
            var isCollidingEntityMovable = collidingEntity.HasAttribute("Movable");
            if (isCollidingEntityMovable)
                return CanMoveEntity(nextPosition, movement);
            return false;
        }

        public void MoveEntity(Vector2 entityPosition, Vector2 movement)
        {
            var previousEntity = (EntityState) null;
            var currentPosition = entityPosition;
            while (_gridTimeline[_timeIndex].HasEntityAt(currentPosition))
            {
                var temp = _gridTimeline[_timeIndex + 1][currentPosition];
                _gridTimeline[_timeIndex + 1][currentPosition] = previousEntity;
                previousEntity = temp.WithNewPosition(currentPosition + movement);
                currentPosition += movement;
            }
            _gridTimeline[_timeIndex + 1][currentPosition] = previousEntity;
        }

        public GridState CurrentGridState => _gridTimeline[_timeIndex];

        public GridState NextGridState => _gridTimeline[_timeIndex + 1];
    }
}
