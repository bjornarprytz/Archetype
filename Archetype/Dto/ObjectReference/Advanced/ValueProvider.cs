using System;
using System.Reflection;

namespace Archetype
{
    public class ValueProvider<R, M> : ValueProvider
        where R : new()
    {
        public override Type ValueType => typeof(M);
        public override Type ReferenceType  => typeof(R);

        public ValueProvider(string memberName)
        {
            MemberName = memberName;
        }

        public M GetValue(R reference)
        {
            return (M) GetPropertyInfo().GetValue(reference);
        }
    }

    public class ValueProvider
    {
        public virtual Type ValueType { get; set; }
        public virtual Type ReferenceType { get; set; }
        public string MemberName { get; set; }


        public PropertyInfo GetPropertyInfo() => ReferenceType?.GetProperty(MemberName);
    }
}
