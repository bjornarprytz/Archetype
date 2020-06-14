using System;
using System.Reflection;

namespace Archetype
{
    public class AttachTriggerActionArgs : ActionInfo
    {
        public EventInfo Event { get; set; }
        public EventHandler<TriggerArgs> Handler { get; set; }


        public AttachTriggerActionArgs(ISource source, ITarget target, EventInfo eventProperty, EventHandler<TriggerArgs> eventHandler) : base(source, target)
        {
            Event = eventProperty;
            Handler = eventHandler;
        }


        protected override void Resolve()
        {
            Event.AddEventHandler(Target, Handler);
        }
    }
}
