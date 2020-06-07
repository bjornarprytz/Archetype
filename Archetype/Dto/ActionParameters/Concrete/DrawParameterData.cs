
namespace Archetype
{
    public class DrawParameterData : ActionParameterData
    {
        public int Strength { get; set; }
        protected override ActionInfo GetActionInfo(Unit source, ITarget target, GameState _) => new DrawActionArgs(source, target as Unit, Strength);
    }
}
