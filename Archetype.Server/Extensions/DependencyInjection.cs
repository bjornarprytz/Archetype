using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Server.Extensions
{
    public static class DependencyInjection
    {
        public static IRequestExecutorBuilder AddLocalTypes(this IRequestExecutorBuilder builder, Assembly assembly)
        {
            foreach (var objectType in assembly.GetAllTypesImplementingOpenGenericType(typeof(ObjectType<>)))
            {
                builder.AddType(objectType);
            }


            return builder;
        }
        
        private static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(this Assembly assembly, Type openGenericType) // TODO: Unscrew this function
        {
            return assembly.GetTypes().SelectMany(type => type.GetInterfaces(), (x, z) => new { x, z })
                .Select(t => new { t, y = t.x.BaseType })
                .Where(t =>
                    (t.y is { IsGenericType: true } &&
                     openGenericType.IsAssignableFrom(t.y.GetGenericTypeDefinition())) || (t.t.z.IsGenericType &&
                        openGenericType.IsAssignableFrom(t.t.z.GetGenericTypeDefinition())))
                .Select(t => t.t.x);
        }
    }
}