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
            var nTypes = 0;

            foreach (var objectType in assembly.GetAllTypesImplementingOpenGenericType(typeof(Archetype<>)))
            {
                builder.AddType(objectType);
                nTypes++;
            }

            foreach (var objectType in assembly.GetAllTypesImplementingOpenGenericType(typeof(InterfaceType<>)))
            {
                builder.AddType(objectType);
                nTypes++;
            }

            foreach (var unionType in assembly.GetAllTypesImplementing<UnionType>())
            {
                builder.AddType(unionType);
                nTypes++;
            }

            Console.WriteLine($"Added {nTypes} types total");
            
            return builder;
        }
        
        private static IEnumerable<Type> GetAllTypesImplementing<T>(this Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(T)));
        }
        
        private static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(this Assembly assembly, Type openGenericType) // TODO: Unscrew this function
        {
            return from x in assembly.GetTypes()
                let y = x.BaseType
                where !x.IsAbstract && !x.IsInterface &&
                      y != null && y.IsGenericType &&
                      y.GetGenericTypeDefinition() == openGenericType
                select x;

        }
    }
}