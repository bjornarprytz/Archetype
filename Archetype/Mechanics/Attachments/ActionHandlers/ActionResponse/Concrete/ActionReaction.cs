using System;

namespace Archetype
{
    public class ActionReaction<TAct> : ActionResponse<ITarget, TAct>
        where TAct : ActionInfo
    {
        public ActionReaction(EventHandler<TAct> handler) : base (handler)
        {
        }

        public override void AttachHandler(ITarget host)
        {
            host.OnTargetOfActionAfter += Handler;
        }

        public override void DetachHandler(ITarget host)
        {
            host.OnTargetOfActionAfter -= Handler;
        }
    }
}
