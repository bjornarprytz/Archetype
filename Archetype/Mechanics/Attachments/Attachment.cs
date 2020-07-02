using System;

namespace Archetype
{
    public abstract class Attachment<THost, TEventArgs>
        where TEventArgs : EventArgs
    {
        protected abstract void Handler(object sender, TEventArgs args);
        public abstract void AttachHandler(THost host);
        public abstract void DetachHandler(THost host);
    }
}
