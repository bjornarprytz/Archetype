using System.Linq;
using Godot;
using Archetype.Godot.Infrastructure;
using Archetype.Prototype1Data;

public class  GameLoader : Node
{
	private ICardFactory _cardFactory;
	
	[Inject]
	public void Construct(IGameView gameView, IGameState gameState, ICardFactory cardFactory)
	{
		gameView.StartGame();
	}
	
	
}
