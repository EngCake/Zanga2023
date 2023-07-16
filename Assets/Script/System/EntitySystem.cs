using UnityEngine;

namespace CakeEngineering
{
    public abstract class EntitySystem : MonoBehaviour
    {
        protected GameManager _gameManager;

        private void Awake()
        {
            _gameManager = GetComponentInParent<GameManager>();
        }

        public abstract void Process(Vector2 playerMovement);
    }
}