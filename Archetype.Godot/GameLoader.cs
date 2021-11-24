using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using Archetype.Client;
using Archetype.Client.State;
using Archetype.Godot.Card;
using Microsoft.Extensions.DependencyInjection;
using StrawberryShake;

public class GameLoader : Node
{
	private IArchetypeGraphQLClient _client;
	private PackedScene _cardScene;

	private IDisposable watcher;
	
	public override async void _Ready()
	{
		// Create Service collection
		// Connect to server
		// Load game state (cards, deck, zones etc)

		_cardScene = ResourceLoader.Load<PackedScene>("res://card.tscn");
		
		_client = CreateClient("localhost:5232/graphql");

		var cardPool = await _client.GetCardPool.ExecuteAsync();
		
		cardPool.EnsureNoErrors();

		var cards = cardPool?.Data?.CardPool?.Sets?.SelectMany(set => set?.Cards).Cast<ICardProtoData>();

		foreach (var (card, i) in cards.Select((c, idx) => (c, idx)))
		{
			var cardNode = _cardScene.Instance() as CardNode;

			cardNode.Load(card);
			cardNode.MoveLocalX(i * 200);
			
			AddChild(cardNode);
			cardNode.Owner = this;
		}

		watcher = _client.OnGameStarted.Watch().Subscribe(message => GD.Print(message?.Data?.OnGameStarted.Message));
	}

	
	private static IArchetypeGraphQLClient CreateClient(string baseUri)
	{
		var serviceCollection = new ServiceCollection();

		serviceCollection
			.AddArchetypeGraphQLClient()
			.ConfigureHttpClient(client => client.BaseAddress = new Uri($"http://{baseUri}"))
			.ConfigureWebSocketClient(client => client.Uri = new Uri($"ws://{baseUri}"));

		var services = serviceCollection.BuildServiceProvider();

		return services.GetRequiredService<IArchetypeGraphQLClient>();
	}
}
