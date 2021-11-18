using System;
using System.Text;

namespace Archetype.Game.Extensions
{
    public static class TypeExtensions
    {
        public static string ReadableFullName(this Type type)
        {
            if (!type.IsGenericType)
                return type.Name;
            
            var sb = new StringBuilder();

            sb.Append(type.Name.Substring(0, type.Name.IndexOf('`')));

            sb.Append('<');

            var first = true;
            foreach (var genericArgument in type.GenericTypeArguments)
            {
                if (!first)
                    sb.Append(',');
                first = false;
                
                sb.Append(genericArgument.ReadableFullName());
            }

            sb.Append('>');

            return sb.ToString();
        }
    }
}