using System;

namespace Archetype
{
    public class ActionFollowUp<THost, TAct> : ActionResponse<THost, TAct>
        where THost : ISource
        where TAct : ActionInfo
    {
        public ActionFollowUp(EventHandler<TAct> handler) : base(handler)
        {
        }

        public override void AttachHandler(THost host)
        {
            host.OnSourceOfActionAfter += Handler;
        }

        public override void DetachHandler(THost host)
        {
            host.OnSourceOfActionAfter -= Handler;
        }
    }
}
