namespace Archetype
{
    public class AttachTriggerActionArgs<THost> : ActionInfo
        where THost : class, ITarget, ITriggerAttachee<THost>
    {
        public Trigger<THost> Trigger { get; set; }

        public AttachTriggerActionArgs(ISource source, THost target, Trigger<THost> trigger) : base(source, target)
        {
            Trigger = trigger;
        }

        protected override void Resolve()
        {
            (Target as THost).AttachTrigger(Trigger);
        }
    }
}
