using System;
using System.Linq;
using System.Reflection;
using Archetype.Godot.Infrastructure;
using Godot;

namespace Archetype.Godot.Extensions;

public static class ServiceProviderExtensions
{
    internal static void ResolveDependencies(this Node node)
    {
        if (node.GetType().GetMethods().FirstOrDefault(m => m.GetCustomAttribute<InjectAttribute>() != null) is not MethodInfo methodInfo) 
            return;

        var parameters = methodInfo.GetParameters().Select(ResolveService).ToArray();

        methodInfo.Invoke(node, parameters);
        
        object ResolveService(ParameterInfo parameterInfo)
        {
            var type = parameterInfo.ParameterType;
		    
            var service = DIContainer.Provider.GetService(type);

            if (service != null) return service;
		    
            throw new Exception($"Unable to resolve service of type {type}");
        }
    }
    
}