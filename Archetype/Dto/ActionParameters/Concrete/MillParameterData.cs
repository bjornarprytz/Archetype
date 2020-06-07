namespace Archetype
{
    public class MillParameterData : ActionParameterData
    {
        public int Strength { get; set; }

        protected override ActionInfo GetActionInfo(Unit source, ITarget target, GameState _) => new MillActionArgs(source, target as Unit, Strength);
    }
}
