namespace Archetype
{
    public class CopyCardParameterData : ActionParameterData
    {
        protected override ActionInfo GetActionInfo(Unit source, ITarget target, GameState gameState)
        {
            return new CopyCardActionArgs(source, target as Card, source, source.Hand);
        }
    }
}
