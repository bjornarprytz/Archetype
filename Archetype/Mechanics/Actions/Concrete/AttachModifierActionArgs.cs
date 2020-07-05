namespace Archetype
{
    public class AttachModifierActionArgs<THost> : ActionInfo
        where THost : class, ITarget, IModifierAttachee<THost>
    {
        public ActionModifier<THost> Modifier { get; set; }

        public AttachModifierActionArgs(ISource source, THost target, ActionModifier<THost> modifier) : base(source, target)
        {
            Modifier = modifier;
        }

        protected override void Resolve()
        {
            (Target as THost).AttachModifier(Modifier);
        }
    }
}
