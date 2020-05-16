
using System;
using System.Collections.Generic;

namespace Archetype
{
    public class EffectModifiers : TypeDictionary<XEffect, int>
    {
        public void Add<T>(int modifier) where T : XEffect => Set<T>(Get<T>() + modifier);
    }
}
