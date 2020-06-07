namespace Archetype
{
    public class DiscardParameterData : ActionParameterData
    {
        public int Strength { get; set; }
        protected override ActionInfo GetActionInfo(Unit source, ITarget target, GameState gameState)
        {
            return new DiscardActionArgs(source, target as Unit, Strength, gameState);
        }
    }
}
