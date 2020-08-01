namespace Archetype
{
    public class CopyCardParameterData : ActionParameterData
    {
        public CardZone TargetZone { get; set; }
        protected override ActionInfo GetActionInfo(ISource source, ITarget target, GameState gameState)
        {
            return new CopyCardActionArgs(source as Unit, target as Card, source as Unit, TargetZone.GetZone(source as Unit));
        }
    }
}
