
using System;
using System.Reflection;

namespace Archetype
{
    public class TriggerParameterData : ActionParameterData
    {
        public EventReferenceData EventData { get; set; }
        public ActionParameterData TriggerAction { get; set; }

        
        protected override ActionInfo GetActionInfo(Unit source, ITarget target, GameState gameState)
        {
            return new AttachTriggerActionArgs(source, target, Event, GetEventHandler(source, target, gameState));
        }
        

        protected EventInfo Event => EventData.GetEventInfo();
        protected EventHandler<TriggerArgs> GetEventHandler(Unit source, ITarget target, GameState gameState)
        {
            return new EventHandler<TriggerArgs>((_, triggerArgs) => FireTrigger(source, target, gameState, triggerArgs));
        }

        protected virtual void FireTrigger(Unit source, ITarget target, GameState gameState, TriggerArgs triggerArgs)
        {
            var targets = SelecetTargets(source, target, gameState, triggerArgs);   

            foreach (var action in TriggerAction.CreateAction(source, targets, gameState))
            {
                // TODO: Use something other than the ActionQueue here; Trigger queue?
                // TODO: How to detach trigger?
                gameState.ActionQueue.Enqueue(action);
            }
        }

        protected virtual ISelectionInfo<ITarget> SelecetTargets(Unit source, ITarget target, GameState gameState, TriggerArgs triggerArgs)
        {
            var targets = TriggerAction.TargetRequirements.GetTargetInfo(source, gameState);

            if (!targets.IsAutomatic) throw new Exception($"Unable to handle non-automatic target selection in triggered actions");

            return targets;
        }
    }
}
