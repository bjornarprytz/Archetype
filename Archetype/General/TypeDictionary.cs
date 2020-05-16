
using System;
using System.Collections.Generic;

namespace Archetype
{


    public class TypeDictionary<KType, VType>
    {
        private Dictionary<Type, VType> _dict = new Dictionary<Type, VType>();

        public virtual bool Has<K>() where K : KType => _dict.ContainsKey(typeof(K));
        public virtual VType Get<K>() where K : KType => Has<K>() ? _dict[typeof(K)] : default;
        public virtual void Set<K>(VType newVal) where K : KType => _dict[typeof(K)] = newVal;

        public virtual void Remove<K>() where K : KType 
        {
            if (Has<K>()) _dict.Remove(typeof(K));
        }

    }

    public class TypeDictionary<VType> : TypeDictionary<VType, VType> { }
}
