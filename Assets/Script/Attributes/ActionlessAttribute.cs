using UnityEngine;

namespace CakeEngineering
{
    [CreateAssetMenu(fileName = "ActionlessAttribute", menuName = "Attributes/Actionless Attribute")]
    public class ActionlessAttribute : EntityAttribute
    {
        public int iterationIndex;

        public string attributeName;

        [TextArea]
        public string description;

        public override int IterationIndex => iterationIndex;

        public override string Name => attributeName;

        public override string Description => description;

        public override void Apply(Entity entity, Vector2 movement, GridState currentGridState, GridState nextGridState) {}
    }
}