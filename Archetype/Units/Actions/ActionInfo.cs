
using System;

namespace Archetype
{
    public abstract class ActionInfo : EventArgs
    {
        public Unit Source { get; protected set; }
        public Unit Target { get; protected set; }
        public int Strength { get; set; }

        public ActionInfo(Unit source, Unit target, int strength)
        {
            Source = source;
            Target = target;
            Strength = strength;
        }

        public void Execute()
        {
            Source.Act(this, Resolve);
        }

        protected abstract void Resolve();
    }
}
