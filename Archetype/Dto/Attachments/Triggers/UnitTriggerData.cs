namespace Archetype
{
    public class UnitTriggerData : ActionParameterData
    {
        public UnitTriggerCause Cause { get; set; }
        public ActionParameterData TriggerAction { get; set; }

        protected override ActionInfo GetActionInfo(ISource source, ITarget target, GameState gameState)
        {
            return new AttachTriggerActionArgs<Unit>(
                source,
                target as Unit,
                Cause.GetTrigger(
                    TriggerAction.CreateTriggerHandler(source, target, gameState)));
        }
    }
}
