using UnityEngine;

namespace CakeEngineering
{
    public abstract class EntityAttribute : ScriptableObject
    {
        public abstract void Apply(Entity entity, Vector2 movement, GridState currentGridState, GridState nextGridState);

        public abstract int IterationIndex { get; }

        public abstract string Name { get; }

        public abstract string Description { get; }
    }
}