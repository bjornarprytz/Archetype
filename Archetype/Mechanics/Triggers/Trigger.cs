namespace Archetype
{
    public abstract class Trigger
    {
        public int Ticks { get; set; }
        protected ITriggerHost Host { get; set; }

        public void Tick()
        {
            AdjustTicks();

            ResolveTick();
        }

        public abstract void AttachTo(ITriggerHost host);
        public abstract void Stack(Trigger other);
        public abstract void Detach();
        protected virtual void AdjustTicks() => Ticks--;
        protected abstract void ResolveTick();
    }
}
