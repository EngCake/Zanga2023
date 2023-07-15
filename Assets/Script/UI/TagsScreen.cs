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
        public GameManager GameManager;

        public TMP_Text PlayerListText;

        public TMP_Text ObjectListText;

        public TMP_Text DescriptionText;

        private Cursor _cursor;

        private PlayerInput _playerInput;

        private InputAction _navigate;

        private InputAction _submit;

        private InputAction _cancel;

        private void Awake()
        {
            _playerInput = new PlayerInput();
            _navigate = _playerInput.UI.Navigate;
            _submit = _playerInput.UI.Submit;
            _cancel = _playerInput.UI.Cancel;
        }

        private void OnEnable()
        {
            _cursor = FindValidCursorPosition(0) ?? FindValidCursorPosition(1);
            ReDraw();
            _navigate.Enable();
            _submit.Enable();
            _cancel.Enable();
        }

        private void OnDisable()
        {
            _navigate.Disable();
            _submit.Disable();
            _cancel.Disable();
        }

        private void ReDraw()
        {
            var player = GameManager.PlayerState;
            var selected = GameManager.SelectedEntityState;
            PlayerListText.text = DrawEntityList(player, 0);
            ObjectListText.text = DrawEntityList(selected, 1);
            if (_cursor != null)
            {
                var attributesList = _cursor.Column == 0 ? player.Attributes : selected.Attributes;
                DescriptionText.text = attributesList[_cursor.Row].Description;
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
                if (attribute.Locked)
                    tags.Add("s");
                if (column == _cursor.Column && i == _cursor.Row)
                    tags.Add("color=yellow");
                var openingTag = $"<{string.Join("><", tags)}>";
                var closingTag = $"</{string.Join("></", tags)}>";
                listText.Append($"- {openingTag}{attribute.Name}{closingTag}");
            }
            return listText.ToString();
        }

        private void Update()
        {
            var navigateDirection = _navigate.ReadValue<Vector2>();
            if (_cursor != null && navigateDirection != Vector2.zero)
            {
                if (navigateDirection == Vector2.up)
                {
                    var newCursor = FindValidCursorPosition(_cursor.Column, _cursor.Row - 1, -1);
                    if (newCursor != null)
                    {
                        _cursor = newCursor;
                        ReDraw();
                    }
                }
                else if (navigateDirection == Vector2.down)
                {
                    var newCursor = FindValidCursorPosition(_cursor.Column, _cursor.Row + 1, 1);
                    if (newCursor != null)
                    {
                        _cursor = newCursor;
                        ReDraw();
                    }
                }
                else if (navigateDirection == Vector2.left && _cursor.Column == 1)
                {
                    var newCursor = FindValidCursorPosition(0, _cursor.Row, 1) ?? FindValidCursorPosition(0, _cursor.Row, -1);
                    if (newCursor != null)
                    {
                        _cursor = newCursor;
                        ReDraw();
                    }
                }
                else if (navigateDirection == Vector2.right && _cursor.Column == 0)
                {
                    var newCursor = FindValidCursorPosition(1, _cursor.Row, 1) ?? FindValidCursorPosition(1, _cursor.Row, -1);
                    if (newCursor != null)
                    {
                        _cursor = newCursor;
                        ReDraw();
                    }
                }
            }
        }

        private Cursor FindValidCursorPosition(int column, int fromRow = 0, int direction = 1)
        {
            var player = GameManager.PlayerState;
            var selected = GameManager.SelectedEntityState;
            var attributesList = column == 0 ? player.Attributes : selected.Attributes;
            for (var i = fromRow; i < attributesList.Count && i >= 0; i += direction)
                if (attributesList[i].Active && !attributesList[i].Locked)
                    return new Cursor { Column = column, Row = i };
            return null;
        }
    }
}