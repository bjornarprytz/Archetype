
using System.Collections.Generic;

namespace Archetype 
{ 
    public interface IModifierAttachee<THost>
        where THost : class, IModifierAttachee<THost>
    {
        List<ActionModifier<THost>> Modifiers { get; }

        void AttachModifier(ActionModifier<THost> modifier)
        {
            modifier.AttachHandler(this as THost);
            Modifiers.Add(modifier);
        }

        void DetachModifier(ActionModifier<THost> modifier)
        {
            Modifiers.Remove(modifier);
            modifier.DetachHandler(this as THost);
        }
    }
}
