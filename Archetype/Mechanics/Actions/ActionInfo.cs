
using System;

namespace Archetype
{
    public abstract class ActionInfo : EventArgs
    {
        public ISource Source { get; protected set; }
        public ITarget Target { get; protected set; }
        public bool IsCancelled { get; set; }

        public ActionInfo(ISource source, ITarget target)
        {
            Source = source;
            Target = target;
        }

        public void Execute()
        {
            Source.PreActionAsSource(this);
            Target.PreActionAsTarget(this);

            if (!IsCancelled)
            {
                Resolve();
            }

            Target.PostActionAsTarget(this);
            Source.PostActionAsSource(this);
        }

        protected abstract void Resolve();
    }
}
