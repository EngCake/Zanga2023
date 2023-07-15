using UnityEngine;

namespace CakeEngineering
{
    [CreateAssetMenu(fileName = "ActionlessAttribute", menuName = "Attributes/Actionless Attribute")]
    public class EntityAttribute : ScriptableObject
    {
        public string Name;

        [TextArea]
        public string Description;

        public bool Locked;

        public bool Active;
    }
}