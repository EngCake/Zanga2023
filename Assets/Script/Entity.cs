using System.Collections.Generic;
using UnityEngine;

namespace CakeEngineering
{
    public class Entity : MonoBehaviour
    {

        [SerializeField]
        private List<EntityAttribute> _initialAttributes;

        private Transform _transform;

        private EntityState _initialState;

        private SpriteRenderer _spriteRenderer;

        [SerializeField]
        private Sprite _normalSprite;

        [SerializeField]
        private Sprite _burningSprite;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _initialState = new EntityState(this, _initialAttributes);
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.sprite = _normalSprite;
        }

        public void Hide()
        {
            LeanTween.color(gameObject, Color.clear, 0.2f);
        }

        public void Show()
        {
            LeanTween.color(gameObject, Color.white, 0.2f);
        }

        public void SetPosition(Vector2 position)
        {
            LeanTween.moveLocal(gameObject, position, 0.2f).setEase(LeanTweenType.easeInCubic);
        }

        public void SetBurning(bool isBurning)
        {
            _spriteRenderer.sprite = isBurning ? _burningSprite : _normalSprite;
        }

        public Vector2 Position => _transform.position;

        public EntityState InitialState => _initialState;
    }
}