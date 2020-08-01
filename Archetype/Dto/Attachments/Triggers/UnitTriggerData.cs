namespace Archetype
{
    public class UnitTriggerData : AttachmentData
    {
        public UnitTriggerCause Cause { get; set; }
        public ActionParameterData TriggerAction { get; set; }

        public override ActionInfo GetAttachmentActionInfo(ISource source, ITarget target, GameState gameState)
        {
            return new AttachTriggerActionArgs<Unit>(
                source,
                target as Unit,
                Cause.GetTrigger(
                    TriggerAction.CreateTriggerHandler(source, target, gameState)));
        }
    }
}
