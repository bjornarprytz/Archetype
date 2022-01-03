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

            foreach (var objectType in assembly.GetAllTypesImplementingOpenGenericType(typeof(ObjectType<>)))
            {
                builder.AddType(objectType);
                Console.WriteLine(objectType.Name);
                nTypes++;
            }

            foreach (var objectType in assembly.GetAllTypesImplementingOpenGenericType(typeof(InterfaceType<>)))
            {
                builder.AddType(objectType);
                Console.WriteLine(objectType.Name);
                nTypes++;
            }

            foreach (var unionType in assembly.GetAllTypesImplementing<UnionType>())
            {
                builder.AddType(unionType);
                Console.WriteLine(unionType.Name);
                nTypes++;
            }

            Console.WriteLine($"Added {nTypes} types total");
            
            return builder;
        }
        
        private static IEnumerable<Type> GetAllTypesImplementing<T>(this Assembly assembly)
        {
            return assembly.GetTypes()
                .Where(t => t.IsConcrete() && t.IsSubclassOf(typeof(T)));
        }
        
        private static IEnumerable<Type> GetAllTypesImplementingOpenGenericType(this Assembly assembly, Type openGenericType)
        {
            return assembly
                .GetTypes()
                .Where(IsConcrete)
                .Where(t => t.InheritsOpenGeneric(openGenericType));
        }

        private static bool IsConcrete(this Type type) => !type.IsAbstract && !type.IsInterface;
        
        private static bool InheritsOpenGeneric(this Type type, Type openGenericType)
        {
            var baseType = type.BaseType;

            while (baseType != null)
            {
                if (baseType is { IsGenericType: true } && baseType.GetGenericTypeDefinition() == openGenericType)
                    return true;
                
                baseType = baseType.BaseType;
            }

            return false;
        }
    }
}