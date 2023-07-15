using UnityEngine;

namespace CakeEngineering
{
    [CreateAssetMenu(fileName = "ActionlessAttribute", menuName = "Attributes/Actionless Attribute")]
    public class EntityAttribute : ScriptableObject
    {
        public int IterationIndex;

        public string Name;

        [TextArea]
        public string Description;
    }
}