using Godot;
using System.Linq;
using Archetype.Client;
using Archetype.Godot.Card;
using Archetype.Godot.Infrastructure;
using StrawberryShake;

public class GameLoader : Node
{
	private IArchetypeGraphQLClient _client;
	private PackedScene _cardScene;

	public override void _Ready()
	{
		_cardScene = ResourceLoader.Load<PackedScene>("res://card.tscn");
	}
	
	[Inject]
	public async void Construct(IArchetypeGraphQLClient client)
	{
		_client = client;
		
		var cardPool = await _client.GetCardPool.ExecuteAsync();
		
		cardPool.EnsureNoErrors();

		var cards = cardPool?.Data?.CardPool?.Sets?.SelectMany(set => set?.Cards).Cast<ICardProtoData>();

		foreach (var (card, i) in cards.Select((c, idx) => (c, idx)))
		{
			var c = _cardScene.Instance();
			
			AddChild(c);
			
			c.QueueFree();
		}
		
		
	}
	
	
}
