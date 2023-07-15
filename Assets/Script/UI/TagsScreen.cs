using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CakeEngineering
{
    class Cursor
    {
        public int Column;

        public int Row;
    }

    public class TagsScreen : MonoBehaviour
    {
        private static readonly int PLAYER_COLUMN = 0;

        private static readonly int OBJECT_COLUMN = 1;

        public GameManager GameManager;

        public TMP_Text PlayerListText;

        public TMP_Text ObjectListText;

        public TMP_Text DescriptionText;

        private Cursor _cursor;

        private PlayerInput _playerInput;

        private InputAction _navigate;

        private InputAction _select;

        private InputAction _cancel;

        private bool _dirty;

        private bool _lock;

        private void Awake()
        {
            _playerInput = new PlayerInput();
            _navigate = _playerInput.UI.Navigate;
            _select = _playerInput.UI.Select;
            _cancel = _playerInput.UI.Cancel;
            _dirty = false;
            _lock = false;
        }

        private void OnEnable()
        {
            _cursor = FindValidCursorPosition(PLAYER_COLUMN) ?? FindValidCursorPosition(OBJECT_COLUMN);
            _navigate.Enable();
            _select.Enable();
            _cancel.Enable();
            Redraw();
        }

        private void OnDisable()
        {
            _navigate.Disable();
            _select.Disable();
            _cancel.Disable();
        }

        private void Redraw()
        {
            var player = GameManager.NextGridState.PlayerState;
            var selected = GameManager.NextGridState.FindState(GameManager.SelectedEntity);
            PlayerListText.text = DrawEntityList(player, PLAYER_COLUMN);
            ObjectListText.text = DrawEntityList(selected, OBJECT_COLUMN);
            if (_cursor != null)
            {
                var attributesList = _cursor.Column == PLAYER_COLUMN ? player.Attributes : selected.Attributes;
                DescriptionText.text = attributesList[_cursor.Row].Description;
            }
            else
            {
                DescriptionText.text = "";
            }
        }

        private string DrawEntityList(EntityState entityState, int column)
        {
            var name = entityState.Name;
            var attributes = entityState.Attributes;
            var listText = new StringBuilder($"<size=60>{name}</size>\n\n");
            for (var i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var tags = new List<string>();
                if (attribute.Locked)
                {
                    tags.Add("color=grey");
                    tags.Add("i");
                }
                if (!attribute.Active)
                    tags.Add("s");
                if (_cursor != null && column == _cursor.Column && i == _cursor.Row)
                    tags.Add("color=yellow");
                var openingTag = tags.Count > 0 ? $"<{string.Join("><", tags)}>" : "";
                var closingTag = tags.Count > 0 ? $"</{string.Join("></", tags)}>" : "";
                listText.Append($"- {openingTag}{attribute.Name}{closingTag}\n");
            }
            return listText.ToString();
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
            var navigateDirection = _navigate.ReadValue<Vector2>();
            if (!_lock && _cursor != null && navigateDirection != Vector2.zero)
            {
                if (navigateDirection == Vector2.up)
                {
                    var newCursor = FindValidCursorPosition(_cursor.Column, _cursor.Row - 1, -1);
                    if (newCursor != null)
                    {
                        _cursor = newCursor;
                        Redraw();
                        Lock();
                    }
                }
                else if (navigateDirection == Vector2.down)
                {
                    var newCursor = FindValidCursorPosition(_cursor.Column, _cursor.Row + 1, 1);
                    if (newCursor != null)
                    {
                        _cursor = newCursor;
                        Redraw();
                        Lock();
                    }
                }
                else if (navigateDirection == Vector2.left && _cursor.Column == OBJECT_COLUMN)
                {
                    var newCursor = FindValidCursorPosition(PLAYER_COLUMN, _cursor.Row, 1) ?? FindValidCursorPosition(PLAYER_COLUMN, _cursor.Row, -1);
                    if (newCursor != null)
                    {
                        _cursor = newCursor;
                        Redraw();
                        Lock();
                    }
                }
                else if (navigateDirection == Vector2.right && _cursor.Column == PLAYER_COLUMN)
                {
                    var newCursor = FindValidCursorPosition(OBJECT_COLUMN, _cursor.Row, 1) ?? FindValidCursorPosition(OBJECT_COLUMN, _cursor.Row, -1);
                    if (newCursor != null)
                    {
                        _cursor = newCursor;
                        Redraw();
                        Lock();
                    }
                }
            }
            else if (!_lock && _select.IsPressed() && _cursor != null)
            {
                var player = GameManager.NextGridState.PlayerState;
                var selectedObject = GameManager.NextGridState.FindState(GameManager.SelectedEntity);
                var attributesList = _cursor.Column == PLAYER_COLUMN ? player.Attributes : selectedObject.Attributes;

                var selectedAttribute = attributesList[_cursor.Row];

                if (_cursor.Column == PLAYER_COLUMN)
                {
                    var modifiedPlayer = player.WithoutAttribute(selectedAttribute);
                    var modifiedObject = selectedObject.WithAttribute(selectedAttribute);
                    if (modifiedObject != null && modifiedPlayer != null)
                    {
                        GameManager.NextGridState[selectedObject.Position] = modifiedObject;
                        GameManager.NextGridState[player.Position] = modifiedPlayer;
                        _dirty = true;
                        _cursor =   FindValidCursorPosition(PLAYER_COLUMN, _cursor.Column, 1) ??
                                    FindValidCursorPosition(PLAYER_COLUMN, _cursor.Column, -1) ??
                                    FindValidCursorPosition(OBJECT_COLUMN, _cursor.Column, 1) ??
                                    FindValidCursorPosition(OBJECT_COLUMN, _cursor.Column, -1);
                        Redraw();
                        Lock();
                    }
                }
                else
                {
                    var modifiedPlayer = player.WithAttribute(selectedAttribute);
                    var modifiedObject = selectedObject.WithoutAttribute(selectedAttribute);
                    if (modifiedObject != null && modifiedPlayer != null)
                    {
                        GameManager.NextGridState[selectedObject.Position] = modifiedObject;
                        GameManager.NextGridState[player.Position] = modifiedPlayer;
                        _dirty = true;
                        _cursor = FindValidCursorPosition(OBJECT_COLUMN, _cursor.Column, 1) ??
                                    FindValidCursorPosition(OBJECT_COLUMN, _cursor.Column, -1) ??
                                    FindValidCursorPosition(PLAYER_COLUMN, _cursor.Column, 1) ??
                                    FindValidCursorPosition(PLAYER_COLUMN, _cursor.Column, -1);
                        Redraw();
                        Lock();
                    }
                }
            }
            else if (!_lock && _cancel.IsPressed())
            {
                gameObject.SetActive(false);
                GameManager.EnablePlayerControl();
                if (_dirty)
                {
                    GameManager.Step();
                }
            }
        }

        private Cursor FindValidCursorPosition(int column, int fromRow = 0, int direction = 1)
        {
            var player = GameManager.NextGridState.PlayerState;
            var selected = GameManager.NextGridState.FindState(GameManager.SelectedEntity);
            var attributesList = column == 0 ? player.Attributes : selected.Attributes;
            for (var i = Mathf.Clamp(fromRow, 0, attributesList.Count - 1); i < attributesList.Count && i >= 0; i += direction)
                if (attributesList[i].Active && !attributesList[i].Locked)
                    return new Cursor { Column = column, Row = i };
            return null;
        }
    }
}