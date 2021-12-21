using System.Linq;
using Godot;
using Archetype.Godot.Infrastructure;
using Archetype.Play;

public class GameLoader : Node
{

	private ICardFactory _cardFactory;
	
	
	
	[Inject]
	public void Construct(ICardFactory cardFactory)
	{
		var gameContext = Game.Create();

		var setup = gameContext.Setup();

		var node = setup.Map.Nodes.First();

		var turn = setup.Start(node);

		var card = turn.PlayableCards.First();

		var playCardContext = turn.PlayCard(card);
		
		
	}
	
	
}
