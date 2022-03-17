using Godot;
using Archetype.Prototype1Data;

public class Clearing : Spatial
{
	private readonly ClearingStateMachine _stateMachine;

	public Clearing()
	{
		_stateMachine = new ClearingStateMachine(this);
	}

	public void Load(IMapNode mapNode)
	{

		// TODO: fill in scene with data
	}

	public override void _Input(InputEvent @event)
	{
		_stateMachine.HandleInput(@event);
	}

	public override void _Process(float delta)
	{
		_stateMachine.Process(delta);
	}

	private void OnMouseEntered()
	{
		_stateMachine.MouseEntered();
	}
	
	private void OnMouseExited()
	{
		_stateMachine.MouseExited();
	}
}







