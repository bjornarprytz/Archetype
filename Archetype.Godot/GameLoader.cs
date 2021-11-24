using Godot;
using System.Linq;
using Archetype.Client;
using Archetype.Godot.Infrastructure;
using StrawberryShake;

public class GameLoader : Node
{
	private IArchetypeGraphQLClient _client;

	private ICardFactory _cardFactory;
	
	
	[Inject]
	public async void Construct(ICardFactory cardFactory, IArchetypeGraphQLClient client)
	{
		_client = client;
		_cardFactory = cardFactory;
		
		var cardPool = await _client.GetCardPool.ExecuteAsync();
		
		cardPool.EnsureNoErrors();

		var cards = cardPool?.Data?.CardPool?.Sets?.SelectMany(set => set?.Cards).Cast<ICardProtoData>();

		foreach (var (card, i) in cards.Select((c, idx) => (c, idx)))
		{
			var c = _cardFactory.CreateCard(card);
			
			AddChild(c);
		}
		
		
	}
	
	
}
