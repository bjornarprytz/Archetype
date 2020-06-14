using System;
using System.Reflection;

namespace Archetype
{
    public class ReferenceData
    {
        public Type MemberType { get; set; }
        public Type ReferenceType { get; set; }
        public string MemberName { get; set; }

        public PropertyInfo GetPropertyInfo() => ReferenceType.GetProperty(MemberName);
    }
}
