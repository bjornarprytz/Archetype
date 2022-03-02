using Godot;
using Archetype.Godot.Infrastructure;

public class MainMenuController : CanvasLayer
{
	private ISceneFactory _sceneFactory;
	
	[Inject]
	public void Construct(ISceneFactory sceneFactory)
	{
		_sceneFactory = sceneFactory;
	}

	private void StartGame()
	{
		var gameNode = _sceneFactory.CreateNode<GameLoader>();
		
		GetParent().AddChild(gameNode);
		QueueFree();
	}
}

