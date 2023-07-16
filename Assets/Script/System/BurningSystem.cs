using UnityEngine;

namespace CakeEngineering
{
    public class BurningSystem : EntitySystem
    {
        [SerializeField]
        private EntityAttribute _burningAttribute;

        public override void Process()
        {
            var currentState = (LevelState) _gameManager.CurrentGridState.Clone();
            var nextState = _gameManager.CurrentGridState;
            foreach (var entityState in currentState.FindByAttribute("Burning"))
            {
                var adjacentEntities = currentState.GetAdjacent4(entityState.Position);
                foreach (var adjacentEntity in adjacentEntities)
                    if (adjacentEntity.HasAttribute("Flammable"))
                        nextState[adjacentEntity.Position] = adjacentEntity.WithAttribute(_burningAttribute);
            }
            foreach (var entityState in currentState.FindByAttribute("Burning"))
                if (!entityState.HasAttribute("Unextinguishable"))
                    nextState[entityState.Position] = null;
        }
    }
}