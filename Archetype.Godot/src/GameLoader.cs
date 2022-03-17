using Godot;
using Archetype.Godot.Infrastructure;
using Archetype.Prototype1Data;

public class GameLoader : Spatial
{
	private IGameView _gameView;
	
	[Inject]
	public void Construct(IGameView gameView)
	{
		_gameView = gameView;
	}

	public override void _Ready()
	{
		base._Ready();
		
		_gameView.StartGame();

		var map = GetNode<MapController>("Map");
		
		map.Load(_gameView.GameState.Map);
	}
}
