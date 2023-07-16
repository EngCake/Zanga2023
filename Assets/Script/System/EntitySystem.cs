using UnityEngine;

namespace CakeEngineering
{
    public abstract class EntitySystem : MonoBehaviour
    {
        protected LevelManager _gameManager;

        private void Awake()
        {
            _gameManager = GetComponentInParent<LevelManager>();
        }

        public abstract void Process();
    }
}