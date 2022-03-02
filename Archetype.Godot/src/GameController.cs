using Godot;
using System;
using Archetype.Godot.Infrastructure;
using Archetype.Prototype1Data;

public class GameController : Control
{
	private IGameView _gameView;
	
	[Inject]
	public void Construct(IGameView gameView)
	{
		_gameView = gameView;
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	private void EndTurn()
	{
		// Replace with function body.
		_gameView.EndTurn();
	}
}



