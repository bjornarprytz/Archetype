
namespace Archetype
{
    public class DrawParameterData : ActionParameterData
    {
        public ValueDescriptor<int> Strength { get; set; }
        protected override ActionInfo GetActionInfo(Unit source, ITarget target, GameState gameState)
        {
            return new DrawActionArgs(source, target as Unit, Strength.CreateGetter(source, gameState));
        }
    }
}
