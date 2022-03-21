using Archetype.Prototype1Data;
using Godot;

namespace Archetype.Godot.Clearing;

public class ClearingNode : Spatial
{
	private readonly ClearingStateMachine _stateMachine;
	private MeshInstance _highlightMesh;
	
	public ClearingNode()
	{
		_stateMachine = new ClearingStateMachine(this);
	}

	public void Load(IMapNode mapNode)
	{
		// TODO: fill in scene with data
	}

	public override void _Ready()
	{
		base._Ready();
		_highlightMesh = GetNode<MeshInstance>("Outline");
	}

	public override void _Input(InputEvent @event)
	{
		_stateMachine.HandleInput(@event);
	}

	public override void _Process(float delta)
	{
		_stateMachine.Process(delta);
	}

	public void HighlightOn()
	{
		_highlightMesh.Visible = true;
	}

	public void HighlightOff()
	{
		_highlightMesh.Visible = false;
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
