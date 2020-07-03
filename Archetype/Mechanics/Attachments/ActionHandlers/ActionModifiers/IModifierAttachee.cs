
namespace Archetype 
{ 
    public interface IModifierAttachee<THost>
        where THost : class, IModifierAttachee<THost>
    {
        TypeDictionary<ActionModifier<THost>> ActionModifiers { get; }

        void AttachModifier<TMod>(TMod modifier) where TMod : ActionModifier<THost>
        {
            if (ActionModifiers.Has<TMod>())
            {
                ActionModifiers.Get<TMod>().StackModifiers(modifier);
            }
            else
            {
                modifier.AttachHandler(this as THost);
                ActionModifiers.Set<TMod>(modifier);
            }
        }

        void DetachModifier<TMod>() where TMod : ActionModifier<THost>
        {
            var modifier = ActionModifiers.Get<TMod>();

            ActionModifiers.Remove<TMod>();

            modifier?.DetachHandler(this as THost);
        }
    }
}
