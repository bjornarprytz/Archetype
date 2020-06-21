namespace Archetype
{
    public class MillParameterData : ActionParameterData
    {
        public ValueDescriptor<int> Strength { get; set; }
        protected override ActionInfo GetActionInfo(Unit source, ITarget target, GameState gameState)
        {
            return new MillActionArgs(source, target as Unit, Strength.CreateGetter(source, gameState));
        }
    }
}
