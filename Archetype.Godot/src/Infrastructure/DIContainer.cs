using Godot;
using System;
using Archetype.Godot.Extensions;
using Archetype.Godot.Infrastructure;
using Archetype.Prototype1Data;
using Microsoft.Extensions.DependencyInjection;

public class DIContainer : Node
{
	// Depends on an auto loaded singleton in (Godot->Project->Project Settings...->AutoLoad) [docs: https://docs.godotengine.org/en/stable/tutorials/scripting/singletons_autoload.html]
	public static IServiceProvider Provider { get; private set; }

	public override void _EnterTree()
	{
		var container = new ServiceCollection();
		Install(container);
		Provider = container.BuildServiceProvider();
	}
	
	private void Install(IServiceCollection container)
	{
		container
			.AddSingleton<ICardFactory, CardFactory>()
			.AddPrototype1()
			.AddSceneFactories()
			;
	}
}
