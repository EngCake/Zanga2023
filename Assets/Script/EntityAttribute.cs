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

        public EntityAttribute(string name, string description, bool locked)
        {
            Name = name;
            Description = description;
            Locked = locked;
        }
    }
}