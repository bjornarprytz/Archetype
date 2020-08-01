using System;

namespace Archetype
{
    public abstract class ActionResponse<THost> : Attachment<THost, ActionInfo>
    {

    }

    public abstract class ActionResponse<THost, TAct> : ActionResponse<THost>
        where TAct : ActionInfo
    {
        private readonly EventHandler<TAct> _handler;

        protected ActionResponse(EventHandler<TAct> handler)
        {
            _handler = handler;
        }

        protected override void Handler(object sender, ActionInfo actionInfo)
        {
            if (!(actionInfo is TAct)) return;

            _handler(sender, actionInfo as TAct);
        }
    }
}
