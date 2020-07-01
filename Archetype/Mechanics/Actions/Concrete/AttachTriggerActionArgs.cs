namespace Archetype
{
    public class AttachTriggerActionArgs : ActionInfo
    {
        public Trigger Trigger { get; set; }

        public AttachTriggerActionArgs(ISource source, ITarget target, Trigger trigger) : base(source, target)
        {
            Trigger = trigger;
        }

        protected override void Resolve()
        {
            Target.AttachTrigger(Trigger);
        }
    }
}
