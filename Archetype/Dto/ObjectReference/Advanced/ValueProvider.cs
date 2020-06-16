using System;
using System.Reflection;

namespace Archetype
{
    public class ValueProvider<R, V> : ValueProvider<V>
    {
        public ValueProvider(string memberName) : base(typeof(R), memberName) { }
        public V GetValue(R reference)
        {
            return (V) GetPropertyInfo().GetValue(reference);
        }
    }

    public class ValueProvider<V> : ValueProvider
    {

        public ValueProvider(Type referenceType, string memberName) : base (referenceType, typeof(V), memberName) { }

        public V GetValue(object reference)
        {
            return (V) GetPropertyInfo().GetValue(reference);
        }

    }

    public class ValueProvider
    {
        public Type ReferenceType { get; set; }
        public Type ValueType { get; set; }
        public string MemberName { get; set; }

        public ValueProvider(Type referenceType, Type valueType, string memberName)
        {
            ReferenceType = referenceType;
            ValueType = valueType;
            MemberName = memberName;
        }

        public PropertyInfo GetPropertyInfo() => ReferenceType?.GetProperty(MemberName);
    }
}
