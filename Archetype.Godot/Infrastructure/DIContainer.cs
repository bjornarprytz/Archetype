using Godot;
using System;
using System.Linq;
using System.Reflection;
using Archetype.Godot.Extensions;
using Archetype.Godot.Infrastructure;
using Godot.Collections;
using Microsoft.Extensions.DependencyInjection;


// Should be the top level node in its scene
public abstract class DIContainer : Node
{
	private readonly IServiceCollection _container = new ServiceCollection();
	private IServiceProvider _provider;

	private Dictionary<string, PackedScene> _packedScenes = new();
	
	public override void _Ready()
	{
		Install(_container);
		_provider = _container.BuildServiceProvider();

		foreach (var child in this.GetChildren<Node>())
		{
			if (child.GetType().GetMethods().FirstOrDefault(m => m.GetCustomAttribute<InjectAttribute>() != null) is not MethodInfo methodInfo) 
				continue;

			var parameters = methodInfo.GetParameters().Select(ResolveService).ToArray();

			methodInfo.Invoke(child, parameters);
		}
	}
	
	protected abstract void Install(IServiceCollection container);

	private object ResolveService(ParameterInfo parameterInfo)
	{
		var type = parameterInfo.ParameterType;
		
		var service = _provider.GetService(type);

		if (service != null) return service;
		
		throw new Exception($"Unable to resolve service of type {type}");
	}
}
