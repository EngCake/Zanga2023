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

        [SerializeField]
        private Entity _selectedEntity;

        [SerializeField]
        private GameObject _selectBox;

        private PlayerInput _playerInput;

        private InputAction _move;

        private InputAction _undo;

        private InputAction _select;

        private bool _lock;

        private List<Entity> _entities;

        private bool _isInSelectState;

        private Vector2 _selectDirection;

        private void Awake()
        {
            _lock = false;
            _playerInput = new PlayerInput();
            _move = _playerInput.Player.Move;
            _undo = _playerInput.Player.Undo;
            _select = _playerInput.Player.Select;
            _isInSelectState = false;
            _selectDirection = Vector2.zero;
        }

        private void Start()
        {
            var entities = GetComponentsInChildren<Entity>();
            _entities = entities.ToList();
            var initialState = new GridState(entities);
            _gridTimeline.Add(initialState);
            _selectBox.SetActive(false);
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
            _select.Enable();
        }

        public void DisablePlayerControler()
        {
            _move.Disable();
            _undo.Disable();
            _select.Disable();
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
            var playerMovement = _move.ReadValue<Vector2>();
            if (!_lock && playerMovement != Vector2.zero && (playerMovement.x == 0 || playerMovement.y == 0))
            {
                if (!_isInSelectState)
                {
                    CreateNextState();
                    RunAllSystems(playerMovement);
                    Step();
                    Lock();
                }
                else
                {
                    var playerPosition = CurrentGridState.PlayerState.Position;
                    if (CurrentGridState.HasEntityAt(playerPosition + playerMovement))
                    {
                        if (!_selectBox.activeSelf)
                            _selectBox.SetActive(true);
                        _selectDirection = playerMovement;
                        _selectBox.transform.position = playerPosition + _selectDirection;
                        Lock();
                    }
                }
            }
            else if (!_lock && _undo.IsPressed())
            {
                if (!_isInSelectState && _gridTimeline.Count > 1)
                {
                    Undo();
                    Lock();
                }
                else
                {
                    _isInSelectState = false;
                    _selectBox.SetActive(false);
                    Lock();
                }
            }
            else if (!_lock && _select.IsPressed())
            {
                if (_isInSelectState && _selectDirection != Vector2.zero)
                {
                    var playerPosition = CurrentGridState.PlayerState.Position;
                    _selectedEntity = CurrentGridState[playerPosition + _selectDirection].Entity;
                    CreateNextState();
                    DisablePlayerControler();
                    _tagsScreen.gameObject.SetActive(true);
                    _selectBox.SetActive(false);
                    _isInSelectState = false;
                }
                else
                {
                    _selectDirection = Vector2.zero;
                    _isInSelectState = true;
                    Lock();
                }
            }
        }

        public void Step()
        {
            _timeIndex++;
            UpdateAllEntities();
        }

        public void Undo()
        {
            if (_timeIndex > 0)
            {
                _timeIndex--;
                UpdateAllEntities();
            }
        }

        private void RunAllSystems(Vector2 playerMovement)
        {
            foreach (var system in _systems)
                system.Process(playerMovement);
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

        public GridState CurrentGridState => _gridTimeline[_timeIndex];

        public GridState NextGridState => _gridTimeline[_timeIndex + 1];

        public Entity SelectedEntity => _selectedEntity;
    }
}
