namespace Archetype
{
    public abstract class OffensiveActionModifier<THost, TAct> : ActionModifier<THost, TAct>
        where THost : class, ISource, IModifierAttachee<THost>
        where TAct : ModifiableActionInfo
    {
        protected OffensiveActionModifier(int modifier=0, float multiplier=1f) : base(modifier, multiplier)
        {
        }

        public override void AttachHandler(THost host)
        {
            host.OnSourceOfActionBefore += Handler;
        }

        public override void DetachHandler(THost host)
        {
            host.OnSourceOfActionBefore -= Handler;
        }
        
    }
}
