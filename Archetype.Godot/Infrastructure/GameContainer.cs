using System;
using Archetype.Prototype1Data;
using Microsoft.Extensions.DependencyInjection;

namespace Archetype.Godot.Infrastructure
{
	public class GameContainer : DIContainer
	{
		protected override void Install(IServiceCollection container)
		{
			container
				.AddSingleton<ICardFactory, CardFactory>()
				.AddPrototype1()
				/*
				 * 
				.AddArchetypeGraphQLClient()
				.ConfigureHttpClient(client => client.BaseAddress = new Uri($"http://localhost:5232/graphql"))
				.ConfigureWebSocketClient(client => client.Uri = new Uri($"ws://localhost:5232/graphql"))
				 */
				;
		}
	}
}
