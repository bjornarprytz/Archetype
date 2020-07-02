using System;

namespace Archetype
{
    public abstract class CustomActionHandler<THost, TAct> : ActionHandler<THost>
        where TAct : ActionInfo
    {
        private EventHandler<TAct> _handler;

        public CustomActionHandler(EventHandler<TAct> handler)
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
