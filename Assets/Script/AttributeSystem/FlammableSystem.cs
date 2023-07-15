namespace CakeEngineering
{
    public class FlammableSystem : EntityAttributeSystem
    {
        public override void Process()
        {
            var currentState = _gameManager.CurrentGridState;
            foreach (var entityState in currentState.FindByAttribute("Flammable"))
            {
                var adjacentEntities = currentState.GetAdjacent4(entityState.Position);
                foreach (var adjacentEntity in adjacentEntities)
                {
                }
            }
        }
    }
}