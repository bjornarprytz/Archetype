
using System;
using System.Collections.Generic;

namespace Archetype
{
    public class EffectModifiers
    {
        private Dictionary<Type, int> _modifiers { get; set; }

        public EffectModifiers()
        {
            _modifiers = new Dictionary<Type, int>();
        }

        public int Get<T>() where T : XEffect => _modifiers.ContainsKey(typeof(T)) ? _modifiers[typeof(T)] : 0;
        public void Set<T>(int modifier) where T : XEffect => _modifiers[typeof(T)] = modifier;
        public void Add<T>(int modifier) where T : XEffect => Set<T>(Get<T>() + modifier);
    }
}
