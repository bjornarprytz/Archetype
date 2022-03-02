using Godot;
using System;
using Archetype.Godot.Infrastructure;
using Archetype.Prototype1Data;

public class MapController : Spatial
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
		var path = GetNode<ForestPath>("ForestPath");

		foreach (var node in _gameView.GameState.Map.Nodes)
		{
			path.AddNode(node);
		}
	}
}
