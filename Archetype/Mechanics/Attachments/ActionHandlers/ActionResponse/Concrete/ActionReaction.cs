using System;

namespace Archetype
{
    public class ActionReaction<THost, TAct> : ActionResponse<THost, TAct>
        where THost : ITarget
        where TAct : ActionInfo
    {
        public ActionReaction(EventHandler<TAct> handler) : base (handler)
        {
        }

        public override void AttachHandler(THost host)
        {
            host.OnTargetOfActionAfter += Handler;
        }

        public override void DetachHandler(THost host)
        {
            host.OnTargetOfActionAfter -= Handler;
        }
    }
}
