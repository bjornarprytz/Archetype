namespace Archetype
{
    public class AttachTriggerActionArgs : ActionInfo
    {
        public Trigger<Unit> Trigger { get; set; }
        public AttachTriggerActionArgs(ISource source, Unit target, Trigger<Unit> trigger) : base(source, target)
        {
            Trigger = trigger;
        }

        protected override void Resolve()
        {
            (Target as Unit).AttachTrigger(Trigger);
        }
    }
}
