using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace CakeEngineering
{
    class Cursor
    {
        public int Column;

        public int Row;
    }

    public class AttributesScreen : MonoBehaviour
    {
        private static readonly int FIRST_COLUMN = 0;

        private static readonly int SECOND_COLUMN = 1;

        [SerializeField]
        private LevelManager _levelManager;

        [SerializeField]
        private PlayerInput _playerInput;

        public Entity firstEntity;

        public Entity secondEntity;

        public TMP_Text firstListText;

        public TMP_Text secondListText;

        private Cursor _cursor;

        private float _lastInputTime;

        private void Start()
        {
            _lastInputTime = Time.time;
        }

        private void OnEnable()
        {
            _cursor = FindValidCursorPosition(FIRST_COLUMN) ?? FindValidCursorPosition(SECOND_COLUMN);
            Redraw();
        }

        private void Redraw()
        {
            var firstEntityState = _levelManager.CurrentGridState.FindState(firstEntity);
            var secondEntityState = _levelManager.CurrentGridState.FindState(secondEntity);
            firstListText.text = DrawEntityList(firstEntityState, FIRST_COLUMN);
            secondListText.text = DrawEntityList(secondEntityState, SECOND_COLUMN);
        }

        private string DrawEntityList(EntityState entityState, int column)
        {
            var name = entityState.Entity.Name;
            var attributes = entityState.Attributes;
            var listText = new StringBuilder($"<size=25>{name}</size>\n\n");
            for (var i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                var tags = new List<string>();
                if (attribute.Locked)
                {
                    tags.Add("color=grey");
                    tags.Add("i");
                }
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
            _lastInputTime = Time.time;
        }

        private bool CooldownIsActive()
        {
            return Time.time - _lastInputTime <= 0.13f;
        }

        public void Navigate(CallbackContext callbackContext)
        {
            if (CooldownIsActive() || callbackContext.phase != InputActionPhase.Started)
                return;
            var navigateDirection = callbackContext.ReadValue<Vector2>();
            if (_cursor != null && navigateDirection != Vector2.zero)
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
                else if (navigateDirection == Vector2.left && _cursor.Column == SECOND_COLUMN)
                {
                    var newCursor = FindValidCursorPosition(FIRST_COLUMN, _cursor.Row, 1) ?? FindValidCursorPosition(FIRST_COLUMN, _cursor.Row, -1);
                    if (newCursor != null)
                    {
                        _cursor = newCursor;
                        Redraw();
                        Lock();
                    }
                }
                else if (navigateDirection == Vector2.right && _cursor.Column == FIRST_COLUMN)
                {
                    var newCursor = FindValidCursorPosition(SECOND_COLUMN, _cursor.Row, 1) ?? FindValidCursorPosition(SECOND_COLUMN, _cursor.Row, -1);
                    if (newCursor != null)
                    {
                        _cursor = newCursor;
                        Redraw();
                        Lock();
                    }
                }
            }
        }

        public void MoveAttributes(CallbackContext callbackContext)
        {
            if (CooldownIsActive() || callbackContext.phase != InputActionPhase.Started)
                return;
            var firstEntityState = _levelManager.CurrentGridState.FindState(firstEntity);
            var secondEntityState = _levelManager.CurrentGridState.FindState(secondEntity);
            var attributesList = _cursor.Column == FIRST_COLUMN ? firstEntityState.Attributes : secondEntityState.Attributes;
            var selectedAttribute = attributesList[_cursor.Row];
            if (_cursor.Column == FIRST_COLUMN)
            {
                var modifiedFirstEntity = firstEntityState.WithoutAttribute(selectedAttribute);
                var modifiedSecondEntity = secondEntityState.WithAttribute(selectedAttribute);
                if (modifiedSecondEntity != null && modifiedFirstEntity != null)
                {
                    _levelManager.CurrentGridState[secondEntityState.Position] = modifiedSecondEntity;
                    _levelManager.CurrentGridState[firstEntityState.Position] = modifiedFirstEntity;
                    _cursor = FindValidCursorPosition(FIRST_COLUMN, _cursor.Column, 1) ??
                                FindValidCursorPosition(FIRST_COLUMN, _cursor.Column, -1) ??
                                FindValidCursorPosition(SECOND_COLUMN, _cursor.Column, 1) ??
                                FindValidCursorPosition(SECOND_COLUMN, _cursor.Column, -1);
                    Redraw();
                    Lock();
                }
            }
            else
            {
                var modifiedFirstState = firstEntityState.WithAttribute(selectedAttribute);
                var modifiedSecondState = secondEntityState.WithoutAttribute(selectedAttribute);
                if (modifiedSecondState != null && modifiedFirstState != null)
                {
                    _levelManager.CurrentGridState[secondEntityState.Position] = modifiedSecondState;
                    _levelManager.CurrentGridState[firstEntityState.Position] = modifiedFirstState;
                    _cursor = FindValidCursorPosition(SECOND_COLUMN, _cursor.Column, 1) ??
                                FindValidCursorPosition(SECOND_COLUMN, _cursor.Column, -1) ??
                                FindValidCursorPosition(FIRST_COLUMN, _cursor.Column, 1) ??
                                FindValidCursorPosition(FIRST_COLUMN, _cursor.Column, -1);
                    Redraw();
                    Lock();
                }
            }
        }

        public void Cancel(CallbackContext callbackContext)
        {
            if (CooldownIsActive() || callbackContext.phase != InputActionPhase.Started)
                return;
            gameObject.SetActive(false);
            _playerInput.SwitchCurrentActionMap("Player");
        }

        private Cursor FindValidCursorPosition(int column, int fromRow = 0, int direction = 1)
        {
            var firstEntityState = _levelManager.CurrentGridState.FindState(firstEntity);
            var secondEntityState = _levelManager.CurrentGridState.FindState(secondEntity);
            var attributesList = column == FIRST_COLUMN ? firstEntityState.Attributes : secondEntityState.Attributes;
            for (var i = Mathf.Clamp(fromRow, 0, attributesList.Count - 1); i < attributesList.Count && i >= 0; i += direction)
                if (!attributesList[i].Locked)
                    return new Cursor { Column = column, Row = i };
            return null;
        }
    }
}