﻿
namespace Archetype 
{ 
    public interface IModifierAttachee<THost>
    {
        void AttachModifier<TMod>(TMod modifier) where TMod : ActionModifier<THost>;
        void DetachModifier<TMod>() where TMod : ActionModifier<THost>;
    }
}