namespace Archetype
{
    public class HealParameterData : ActionParameterData
    {
        public int Strength { get; set; }

        protected override ActionInfo GetActionInfo(Unit source, ITarget target, GameState _) => new HealActionArgs(source, target as Unit, Strength);
    }
}
