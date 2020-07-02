using System;

namespace Archetype
{
    public class ActionFollowUp<TAct> : ActionResponse<ISource, TAct>
        where TAct : ActionInfo
    {
        public ActionFollowUp(EventHandler<TAct> handler) : base(handler)
        {
        }

        public override void AttachHandler(ISource host)
        {
            host.OnSourceOfActionAfter += Handler;
        }

        public override void DetachHandler(ISource host)
        {
            host.OnSourceOfActionAfter -= Handler;
        }
    }
}
