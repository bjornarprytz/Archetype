namespace Archetype
{
    public class DamageParameterData : ActionParameterData
    {
        public int Strength { get; set; }

        protected override ActionInfo GetActionInfo(Unit source, ITarget target, GameState _) => new DamageActionArgs(source, target as Unit, Strength);
    }
}
