using System;
using System.Reflection;

namespace Archetype
{
    public class AttachTriggerActionArgs<T> : ActionInfo
        where T : TriggerArgs
    {
        public EventInfo Event { get; set; }
        public EventHandler<T> Handler { get; set; }


        public AttachTriggerActionArgs(ISource source, ITarget target, EventInfo eventProperty, EventHandler<T> eventHandler) : base(source, target)
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
