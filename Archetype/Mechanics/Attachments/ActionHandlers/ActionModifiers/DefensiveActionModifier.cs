namespace Archetype
{
    public abstract class DefensiveActionModifier<THost, TAct> : ActionModifier<THost, TAct>
        where THost : class, ITarget, IModifierAttachee<THost>
        where TAct : ModifiableActionInfo
    {
        protected DefensiveActionModifier(int modifier=0, float multiplier=1f) : base (modifier, multiplier)
        {
        }

        public override void AttachHandler(THost host)
        {
            host.OnTargetOfActionBefore += Handler;
        }

        public override void DetachHandler(THost host)
        {
            host.OnTargetOfActionBefore -= Handler;
        }
    }
}
