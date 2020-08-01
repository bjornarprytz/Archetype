namespace Archetype
{
    public class DiscardParameterData : ActionParameterData
    {
        public ValueDescriptor<int> Strength { get; set; }
        protected override ActionInfo GetActionInfo(ISource source, ITarget target, GameState gameState)
        {
            return new DiscardActionArgs(source as Unit, target as Unit, Strength.CreateGetter(source as Unit, gameState));
        }
    }
}
