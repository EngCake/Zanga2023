using UnityEngine;

namespace CakeEngineering
{
    public class BurningSystem : EntityAttributeSystem
    {
        [SerializeField]
        private EntityAttribute _burningAttribute;

        public override void Process(Vector2 playerMovement)
        {
            var currentState = (LayerState) _gameManager.CurrentGridState.Clone();
            var nextState = _gameManager.CurrentGridState;
            foreach (var entityState in currentState.FindByAttribute("Burning"))
            {
                var adjacentEntities = currentState.GetAdjacent4(entityState.Position);
                foreach (var adjacentEntity in adjacentEntities)
                {
                    if (adjacentEntity.HasAttribute("Flammable"))
                    {
                        nextState[adjacentEntity.Position] = adjacentEntity.WithAttribute(_burningAttribute);
                    }
                }
            }
            foreach (var entityState in currentState.FindByAttribute("Burning"))
            {
                if (!entityState.HasAttribute("Permanent Fire"))
                {
                    nextState[entityState.Position] = null;
                }
            }
        }
    }
}