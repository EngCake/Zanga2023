using UnityEngine;

namespace CakeEngineering
{
    [CreateAssetMenu(fileName = "Attribute", menuName = "Attribute")]
    public class EntityAttribute : ScriptableObject
    {
        public string Name;

        public bool Locked;

        public bool Active;
    }
}