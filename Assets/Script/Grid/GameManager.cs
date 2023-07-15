using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using static UnityEditorInternal.VersionControl.ListControl;

namespace CakeEngineering
{
    enum HUD
    {
        None,
        SelectEntity,
        SelectFromMultiple
    }

    class GameState
    {
        public HUD HUD;

        public Vector2 SelectDirection;

        public int SelectIndex;

        public List<EntityState> EntitiesStates;

        public GameState()
        {
            HUD = HUD.None;
            SelectDirection = Vector2.zero;
            SelectIndex = 0;
            EntitiesStates = new List<EntityState>();
        }
    }

    public class GameManager : MonoBehaviour
    {
        [SerializeField]
        private List<EntityAttributeSystem> _systems;

        [SerializeField]
        private TagsScreen _tagsScreen;

        [SerializeField]
        private GameObject _selectBox;

        [SerializeField]
        private TMP_Text _entitiesSelector;

        private History<GridState> _gridHistory;

        private PlayerInput _playerInput;

        private InputAction _move;

        private InputAction _undo;

        private InputAction _select;

        private bool _lock;

        private List<Entity> _entities;

        private GameState _gameState;

        private void Awake()
        {
            _lock = false;
            _playerInput = new PlayerInput();
            _move = _playerInput.Player.Move;
            _undo = _playerInput.Player.Undo;
            _select = _playerInput.Player.Select;
            _gameState = new GameState();
            _gridHistory = new History<GridState>();
        }

        private void Start()
        {
            var entities = GetComponentsInChildren<Entity>();
            _entities = entities.ToList();
            _gridHistory.CreateNext(GridState.InitializeFromEntities(entities));
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
                if (_gameState.HUD == HUD.None)
                {
                    _gridHistory.CreateNext((LayerState) _gridHistory.Current.Clone());
                    RunAllSystems(playerMovement);
                    UpdateAllEntities();
                    Lock();
                }
                else if (_gameState.HUD == HUD.SelectEntity)
                {
                    var playerPosition = CurrentGridState.PlayerState.Position;
                    var statesOfEntitesAtPosition = CurrentGridState.StatesOfEntitesAt(playerPosition + playerMovement);
                    if (statesOfEntitesAtPosition.Count > 0)
                    {
                        if (!_selectBox.activeSelf)
                            _selectBox.SetActive(true);
                        _gameState.SelectDirection = playerMovement;
                        _selectBox.transform.position = playerPosition + playerMovement;
                        Lock();
                    }
                }
                else
                {
                    if (playerMovement == Vector2.up && _gameState.SelectIndex > 0)
                        _gameState.SelectIndex--;
                    else if (playerMovement == Vector2.down && _gameState.SelectIndex + 1 < _gameState.EntitiesStates.Count)
                        _gameState.SelectIndex++;
                }
            }
            else if (!_lock && _undo.IsPressed())
            {
                if (_gameState.HUD == HUD.None && _gridHistory.TryUndo())
                {
                    UpdateAllEntities();
                    Lock();
                }
                else if (_gameState.HUD == HUD.SelectEntity)
                {
                    _gameState.HUD = HUD.None;
                    _selectBox.SetActive(false);
                    Lock();
                }
                else
                {
                    _gameState.HUD = HUD.SelectEntity;
                    _selectBox.SetActive(true);
                    _entitiesSelector.gameObject.SetActive(false);
                    Lock();
                }
            }
            else if (!_lock && _select.IsPressed())
            {
                if (_gameState.HUD == HUD.None)
                {
                    _gameState.SelectDirection = Vector2.zero;
                    _gameState.HUD = HUD.SelectEntity;
                    Lock();
                }
                else if (_gameState.HUD == HUD.SelectEntity)
                {
                    var playerPosition = CurrentGridState.PlayerState.Position;
                    _tagsScreen.firstEntity = CurrentGridState.PlayerState.Entity;
                    var statesOfEntitiesAtPosition = CurrentGridState.StatesOfEntitesAt(playerPosition + _gameState.SelectDirection);
                    if (statesOfEntitiesAtPosition.Count == 1)
                    {
                        _tagsScreen.secondEntity = statesOfEntitiesAtPosition[0].Entity;
                        _gridHistory.CreateNext((LayerState)_gridHistory.Current.Clone());
                        DisablePlayerControler();
                        _tagsScreen.gameObject.SetActive(true);
                        _selectBox.SetActive(false);
                        _gameState.HUD = HUD.None;
                    }
                    else
                    {
                        _gameState.HUD = HUD.SelectFromMultiple;
                        _gameState.EntitiesStates = statesOfEntitiesAtPosition;
                        _selectBox.SetActive(false);
                        _entitiesSelector.gameObject.SetActive(true);
                    }
                }
                else
                {
                    _tagsScreen.firstEntity = CurrentGridState.PlayerState.Entity;
                    _tagsScreen.secondEntity = _gameState.EntitiesStates[_gameState.SelectIndex].Entity;
                    _gridHistory.CreateNext((LayerState)_gridHistory.Current.Clone());
                    DisablePlayerControler();
                    _tagsScreen.gameObject.SetActive(true);
                    _entitiesSelector.gameObject.SetActive(false);
                    _gameState.HUD = HUD.None;
                }
            }
        }

        private void RedrawMultiupleSelector()
        {
            var entitiesList = new StringBuilder();
            for (var i = 0; i < _gameState.EntitiesStates.Count; i++)
            {
                var entityState = _gameState.EntitiesStates[i];
                var openingTag = i == _gameState.SelectIndex ? "<color=yellow>" : "";
                var closingTag = i == _gameState.SelectIndex ? "</color>" : "";
                entitiesList.AppendLine($" - {openingTag}{entityState.Entity.Name}{closingTag}");
            }
            _entitiesSelector.text = entitiesList.ToString();
        }

        public void Undo()
        {
            _gridHistory.TryUndo();
        }

        private void RunAllSystems(Vector2 playerMovement)
        {
            foreach (var system in _systems)
                system.Process(playerMovement);
        }

        private void UpdateAllEntities()
        {
            _entities.ForEach(entity => entity.UpdateCurrentState());
        }

        public GridState CurrentGridState => _gridHistory.Current;
    }
}
