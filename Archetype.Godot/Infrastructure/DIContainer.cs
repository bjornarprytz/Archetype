using Godot;
using System;
using System.Linq;
using System.Reflection;
using Archetype.Godot.Extensions;
using Archetype.Godot.Infrastructure;
using Microsoft.Extensions.DependencyInjection;


// Should be the top level node in its scene
public abstract class DIContainer : Node
{
	private readonly IServiceCollection _container = new ServiceCollection();
	private IServiceProvider _provider;

	public override void _Ready()
	{
		Install(_container);
		_provider = _container.BuildServiceProvider();

		foreach (var child in this.GetChildren<Node>())
		{
			if (child.GetType().GetMethods().FirstOrDefault(m => m.GetCustomAttribute<InjectAttribute>() != null) is not MethodInfo methodInfo) 
				continue;

			var parameters = methodInfo.GetParameters().Select(p => ResolveService(p.ParameterType)).ToArray();

			methodInfo.Invoke(child, parameters);
		}
	}
	
	protected abstract void Install(IServiceCollection container);

	private object ResolveService(Type type)
	{
		var service = _provider.GetService(type);

		if (service != null) return service;
		
		var parent = GetClosestContainer();

		if (parent is null)
			throw new Exception($"Unable to resolve service of type {type}");

		return parent.ResolveService(type);

	}

	private DIContainer GetClosestContainer()
	{
		var parent = GetParent()?.Owner;

		while (parent is not null && parent is not DIContainer)
		{
			parent = parent.GetParent().Owner;
		}
		
		return parent as DIContainer;
	}
}
