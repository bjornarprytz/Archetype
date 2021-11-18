using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using Archetype.Client;
using Archetype.Godot.Card;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;

public class GameLoader : Node
{
	private IArchetypeGraphQLClient _client;
	private PackedScene _cardScene;
	
	public override async void _Ready()
	{
		// Create Service collection
		// Connect to server
		// Load game state (cards, deck, zones etc)

		_cardScene = ResourceLoader.Load<PackedScene>("res://card.tscn");
		
		_client = CreateClient("http://localhost:5232/graphql");

		var cardPool = await _client.GetCardPool.ExecuteAsync();
		
		cardPool.EnsureNoErrors();

		var cards = cardPool?.Data?.CardPool?.Sets?.SelectMany(set => set?.Cards).Cast<IFullCardProtoData>();

		foreach (var (card, i) in cards.Select((c, idx) => (c, idx)))
		{
			var cardNode = _cardScene.Instance() as CardNode;

			cardNode.Load(card);
			cardNode.MoveLocalX(i * 100);
			
			AddChild(cardNode);
		}
	}

	
	private static IArchetypeGraphQLClient CreateClient(string baseUri)
	{
		var serviceCollection = new ServiceCollection();

		serviceCollection
			.AddArchetypeGraphQLClient()
			.ConfigureHttpClient(client => client.BaseAddress = new Uri(baseUri));

		var services = serviceCollection.BuildServiceProvider();

		return services.GetRequiredService<IArchetypeGraphQLClient>();
	}
}
