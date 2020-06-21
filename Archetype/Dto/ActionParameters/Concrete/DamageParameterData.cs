namespace Archetype
{
    public class DamageParameterData : ActionParameterData
    {
        public ValueDescriptor<int> Strength { get; set; }

        protected override ActionInfo GetActionInfo(Unit source, ITarget target, GameState gameState)
        {
            return new DamageActionArgs(source, target as Unit, Strength.CreateGetter(source, gameState));
        }
    }
}
