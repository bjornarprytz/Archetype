using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Archetype.Godot.Infrastructure;
using Godot;

namespace Archetype.Godot.Extensions;

public static class ServiceProviderExtensions
{
    internal static void ResolveDependencies(this Node node)
    {
        if (node.GetType().GetMethods().FirstOrDefault(m => m.GetCustomAttribute<InjectAttribute>() != null) is
            { } methodInfo)
        {
            
            var parameters = methodInfo.GetParameters().Select(ResolveService).ToArray();
            
            methodInfo.Invoke(node, parameters);
        }
        
        foreach (var child in node.GetChildren<Node>())
        {
            child.ResolveDependencies();
        }
        
        object ResolveService(ParameterInfo parameterInfo)
        {
            var type = parameterInfo.ParameterType;
		    
            var service = DIContainer.Provider.GetService(type);

            if (service != null) return service;
		    
            throw new Exception($"Unable to resolve service of type {type}");
        }
    }
    
}