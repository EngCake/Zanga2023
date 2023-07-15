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

        [SerializeField]
        private TagsScreen _tagsScreen;

        private PlayerInput _playerInput;

        private InputAction _move;

        private InputAction _undo;

        private InputAction _viewAttributes;

        private bool _lock;

        private GridState _previousState;

        [SerializeField]
        private Entity _selectedEntity;

        private List<Entity> _entities;

        private void Awake()
        {
            _lock = false;
            _playerInput = new PlayerInput();
            _move = _playerInput.Player.Move;
            _undo = _playerInput.Player.Undo;
            _viewAttributes = _playerInput.Player.ViewAttributes;
            _gridTimeline = new List<GridState>();

            _previousState = null;
            _selectedEntity = null;
        }

        private void Start()
        {
            var entities = GetComponentsInChildren<Entity>();
            _entities = entities.ToList();
            var initialState = new GridState(entities);
            _gridTimeline.Add(initialState);
            UpdateAllEntities();
        }

        private void OnEnable()
        {
            EnablePlayerControl();
        }

        private void OnDisable()
        {
            DisablePlayerControler();
        }

        public void EnablePlayerControl()
        {
            _move.Enable();
            _undo.Enable();
            _viewAttributes.Enable();
        }

        public void DisablePlayerControler()
        {
            _move.Disable();
            _undo.Disable();
            _viewAttributes.Disable();
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
                CreateNextState();
                MovePlayer(moveDirection);
                SystemsProcess();
                _previousState = CurrentGridState;
                _timeIndex++;
                UpdateAllEntities();
                Lock();
            }
            else if (!_lock && _undo.IsPressed() && _gridTimeline.Count > 1)
            {
                if (_timeIndex > 0)
                {
                    _previousState = CurrentGridState;
                    _timeIndex--;
                    UpdateAllEntities();
                    Lock();
                }
            }
        }

        private void SystemsProcess()
        {
            foreach (var system in _systems)
                system.Process();
        }

        private void CreateNextState()
        {
            if (_timeIndex + 1 < _gridTimeline.Count)
                _gridTimeline[_timeIndex + 1] = (GridState)_gridTimeline[_timeIndex].Clone();
            else
                _gridTimeline.Add((GridState)_gridTimeline[_timeIndex].Clone());
        }

        private void UpdateAllEntities()
        {
            _entities.ForEach(entity => entity.UpdateCurrentState());
        }

        public void MovePlayer(Vector2 playerMovement)
        {
            foreach (var entityState in _gridTimeline[_timeIndex])
            {
                if (entityState.HasAttribute("Player"))
                {
                    if (CanMoveEntity(entityState.Position, playerMovement))
                        MoveEntity(entityState.Position, playerMovement);
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
            while (CurrentGridState.HasEntityAt(currentPosition))
            {
                var temp = NextGridState[currentPosition];
                NextGridState[currentPosition] = previousEntity;
                previousEntity = temp.WithNewPosition(currentPosition + movement);
                currentPosition += movement;
            }
            NextGridState[currentPosition] = previousEntity;
        }

        public EntityState FindState(Entity entity)
        {
            return CurrentGridState.FirstOrDefault(entityState => entityState.Entity == entity);
        }

        public EntityState PlayerState => CurrentGridState.First(entityState => entityState.HasAttribute("Player"));

        public EntityState SelectedEntityState => _selectedEntity.InitialState;

        public GridState CurrentGridState => _gridTimeline[_timeIndex];

        public GridState NextGridState => _gridTimeline[_timeIndex + 1];
    }
}
