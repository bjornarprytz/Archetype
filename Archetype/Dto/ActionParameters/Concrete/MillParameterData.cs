namespace Archetype
{
    public class MillParameterData : ActionParameterData
    {
        public ValueDescriptor<int> Strength { get; set; }
        protected override ActionInfo GetActionInfo(ISource source, ITarget target, GameState gameState)
        {
            return new MillActionArgs(source as Unit, target as Unit, Strength.CreateGetter(source as Unit, gameState));
        }
    }
}
