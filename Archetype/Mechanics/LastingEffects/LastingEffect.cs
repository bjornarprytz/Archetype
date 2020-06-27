using System;

namespace Archetype
{
    public abstract class LastingEffect
    {
        public ITarget Target { get; protected set; }
        public int Ticks { get; set; }

        public void Apply(ITarget target)
        {
            Target = target;
        }

        public void Tick()
        {
            AdjustTicks();

            ResolveTick();
        }

        protected virtual void AdjustTicks() => Ticks--;

        protected abstract void ResolveTick();
        protected abstract void Remove();
    }
}
