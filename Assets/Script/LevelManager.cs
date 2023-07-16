using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace CakeEngineering
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField]
        private List<EntitySystem> _systems;

        [SerializeField]
        private AttributesScreen _tagsScreen;

        [SerializeField]
        private GameObject _selectBox;

        [SerializeField]
        private LevelLoader _levelLoader;

        [SerializeField]
        private PlayerInput _playerInput;

        private History<LevelState> _gridHistory;

        private float _lastInputTime;

        private List<Entity> _entities;

        private Vector2 _selectDirection;

        private void Awake()
        {
            _selectDirection = Vector2.zero;
            _gridHistory = new History<LevelState>();
        }

        private void Start()
        {
            _lastInputTime = Time.time;
            var entities = GetComponentsInChildren<Entity>();
            _entities = entities.ToList();
            var initialState = new LevelState(entities);
            _gridHistory.CreateNext(initialState);
            _selectBox.SetActive(false);
            UpdateAllEntities();
        }

        private void Lock()
        {
            _lastInputTime = Time.time;
        }

        private bool CooldownIsActive()
        {
            return Time.time - _lastInputTime <= 0.13f;
        }

        public void MovePlayer(CallbackContext callbackContext)
        {
            if (CooldownIsActive() || callbackContext.phase != InputActionPhase.Started)
                return;
            var movement = callbackContext.ReadValue<Vector2>();
            _gridHistory.CreateNext((LevelState)_gridHistory.Current.Clone());
            CurrentGridState[CurrentGridState.PlayerState.Position] = CurrentGridState.PlayerState.WithVelocity(movement);
            RunAllSystems(movement);
            UpdateAllEntities();
            Lock();
        }

        public void ChooseSelectedEntity(CallbackContext callbackContext)
        {
            if (CooldownIsActive() || callbackContext.phase != InputActionPhase.Started)
                return;
            var movement = callbackContext.ReadValue<Vector2>();
            if (movement != Vector2.zero && (movement.x == 0 || movement.y == 0))
            {
                var playerPosition = CurrentGridState.PlayerState.Position;
                if (CurrentGridState.HasEntityAt(playerPosition + movement))
                {
                    if (!_selectBox.activeSelf)
                        _selectBox.SetActive(true);
                    _selectDirection = movement;
                    _selectBox.transform.position = playerPosition + movement;
                    Lock();
                }
            }
        }

        public void Undo(CallbackContext callbackContext)
        {
            if (CooldownIsActive() || callbackContext.phase != InputActionPhase.Started)
                return;
            _gridHistory.TryUndo();
            UpdateAllEntities();
            Lock();
        }

        public void EnterSelectEntityState(CallbackContext callbackContext)
        {
            if (CooldownIsActive() || callbackContext.phase != InputActionPhase.Started)
                return;
            _playerInput.SwitchCurrentActionMap("Select");
            _selectDirection = Vector2.zero;
            Lock();
        }

        public void ExitSelectEntityState(CallbackContext callbackContext)
        {
            if (CooldownIsActive() || callbackContext.phase != InputActionPhase.Started)
                return;
            _playerInput.SwitchCurrentActionMap("Player");
            if (_selectBox.activeSelf)
                _selectBox.SetActive(false);
            Lock();
        }

        public void SelectEntity(CallbackContext callbackContext)
        {
            if (CooldownIsActive() || callbackContext.phase != InputActionPhase.Started || _selectDirection == Vector2.zero)
                return;
            var playerPosition = CurrentGridState.PlayerState.Position;
            _tagsScreen.firstEntity = CurrentGridState.PlayerState.Entity;
            _tagsScreen.secondEntity = CurrentGridState[playerPosition + _selectDirection].Entity;
            _gridHistory.CreateNext((LevelState)_gridHistory.Current.Clone());
            _tagsScreen.gameObject.SetActive(true);
            _selectBox.SetActive(false);
            _playerInput.SwitchCurrentActionMap("UI");
        }

        private void RunAllSystems(Vector2 playerMovement)
        {
            foreach (var system in _systems)
                system.Process();
        }

        private void UpdateAllEntities()
        {
            _entities.ForEach(entity => entity.UpdateCurrentState());
        }

        public LevelState CurrentGridState => _gridHistory.Current;

        public void Win()
        {
            _levelLoader.TransitionToNextScene();
        }
    }
}
