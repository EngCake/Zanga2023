using UnityEngine;

namespace CakeEngineering
{
    public class FlammableAttribute : EntityAttribute
    {
        public override int IterationIndex => 1;

        public override string Name => "Flammable";

        public override string Description => "Flammable entities burn and gets destroyed when walking over flaming entities.";

        public override void Apply(Entity entity, Vector2 movement, GridState currentGridState, GridState nextGridState)
        {
        }
    }
}