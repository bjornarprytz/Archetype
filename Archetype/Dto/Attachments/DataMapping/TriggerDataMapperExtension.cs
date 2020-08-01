using System;

namespace Archetype
{
    public static class TriggerDataMapperExtension
    {
        public static EventHandler<TriggerArgs> CreateTriggerHandler(this ActionParameterData actionParameterData, ISource source, ITarget target, GameState gameState)
        {
            return (_s, _e) =>
            {
                gameState.ActionQueue.EnqueueActions(
                    actionParameterData.CreateAction(
                        source,
                        actionParameterData.TargetRequirements.GetSelectionInfo(source as Unit, gameState),
                        gameState));
            };
        }

        public static Trigger<Unit> GetTrigger(this UnitTriggerCause cause, EventHandler<TriggerArgs> handler)
        {
            return cause switch
            {
                UnitTriggerCause.Damaged => new DamageTrigger(handler),
                UnitTriggerCause.Healed => new HealTrigger(handler),
                UnitTriggerCause.DiscardedCard => new DiscardTrigger(handler),
                UnitTriggerCause.DrewCard => new DrawTrigger(handler),
                UnitTriggerCause.MilledCard => new MillTrigger(handler),
                UnitTriggerCause.ShuffledDeck => new ShuffleTrigger(handler),
                _ => throw new Exception($"Unhandled UnitTriggerCause: {cause}"),
            };
        }
    }
}
