using System.Collections.Generic;
using UnityEngine;

namespace CakeEngineering
{
    public class Entity : MonoBehaviour
    {
        [Header("Details")]
        [SerializeField]
        private string _entityName;

        [SerializeField]
        private EntityType _entityType;

        [Header("Sprites")]
        [SerializeField]
        private Sprite _normalSprite;

        [SerializeField]
        private Sprite _burningSprite;


        [Header("Initial State")]
        [SerializeField]
        private List<EntityAttribute> _initialAttributes;

        [SerializeField]
        private Vector2 _initialVelocity;

        private Transform _transform;

        private EntityState _initialState;

        private EntityState _currentState;

        private SpriteRenderer _spriteRenderer;

        private LevelManager _gameManager;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _initialState = new EntityState(_initialAttributes, this, Position, _initialVelocity);
            _currentState = _initialState;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = _normalSprite;
            _gameManager = GetComponentInParent<LevelManager>();
        }

        public void UpdateCurrentState()
        {
            var state = _gameManager.CurrentGridState.FindState(this);
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

        public string Name => _entityName;

        public EntityType Type => _entityType;

        public EntityState InitialState => _initialState;

        public Vector2 InitialVelocity => _initialVelocity;
    }
}