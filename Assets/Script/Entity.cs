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

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            _initialState = new EntityState(this, _initialAttributes);
        }

        public void SetPosition(Vector2 position)
        {
            LeanTween.moveLocal(gameObject, position, 0.2f).setEase(LeanTweenType.easeInCubic);
        }

        public Vector2 Position => _transform.position;

        public EntityState InitialState => _initialState;
    }
}