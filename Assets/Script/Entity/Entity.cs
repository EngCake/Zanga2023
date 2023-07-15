using System.Collections.Generic;
using UnityEngine;

namespace CakeEngineering
{
    public class Entity : MonoBehaviour
    {
        [SerializeField]
        private Sprite _normalSprite;

        [SerializeField]
        private Sprite _burningSprite;

        [SerializeField]
        private List<EntityAttribute> _initialAttributes;

        private Transform _transform;

        private EntityState _initialState;

        private EntityState _currentState;

        private SpriteRenderer _spriteRenderer;

        private GameManager _gameManager;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _initialState = new EntityState(this, _initialAttributes);
            _currentState = _initialState;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = _normalSprite;
            _gameManager = GetComponentInParent<GameManager>();
        }

        public void UpdateCurrentState()
        {
            var state = _gameManager.FindState(this);
            if (state == null && _currentState != null)
            {
                LeanTween.color(gameObject, Color.clear, 0.2f);
            }
            else if (state != null && _currentState == null)
            {
                LeanTween.color(gameObject, Color.white, 0.2f);
            }
            if (state != null)
            {
                LeanTween.moveLocal(gameObject, state.Position, 0.2f).setEaseInOutCubic();
                _spriteRenderer.sprite = state.HasAttribute("Burning") ? _burningSprite : _normalSprite;
            }
            _currentState = state;
        }

        public Vector2 Position => _transform.position;

        public EntityState InitialState => _initialState;

        public EntityState CurrentState => _currentState;
    }
}