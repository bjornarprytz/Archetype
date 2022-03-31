using Archetype.Godot.Card;
using Archetype.Godot.Extensions;
using Archetype.Godot.Infrastructure;
using Archetype.Godot.StateMachine;
using Archetype.Prototype1Data;
using Godot;
using Stateless;

namespace Archetype.Godot.Clearing;

public class ClearingNode : Spatial
{
	private MeshInstance _highlightMesh;
	private StateMachine<IState<ClearingNode>, State.Trigger> _stateMachine;


	[Inject]
	public void Construct(
		State.Idle idle,
		State.Highlight highlight
	)
	{
		_stateMachine = new StateMachine<IState<ClearingNode>, State.Trigger>(idle);
		
		_stateMachine.OnTransitioned(state =>
		{
			state.Source.OnExit(this);
			state.Destination.OnEnter(this);
		});
		
		_stateMachine.Configure(idle)
			.Permit(State.Trigger.HoverStart, highlight);

		_stateMachine.Configure(highlight)
			.Permit(State.Trigger.HoverStop, idle);
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
		_stateMachine.State.HandleInput(this, @event);
	}

	public override void _Process(float delta)
	{
		_stateMachine.State.Process(this, delta);
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
		_stateMachine.FireIfPossible(State.Trigger.HoverStart);
	}
	
	private void OnMouseExited()
	{
		_stateMachine.FireIfPossible(State.Trigger.HoverStop);
	}
	
	public sealed class State
	{
		public enum Trigger
		{
			HoverStart,
			HoverStop,
		}
		
		public class Idle : State<ClearingNode>
		{ }
		
		public class Highlight : State<ClearingNode>
		{
			public override void OnEnter(ClearingNode model)
			{ 
				model.HighlightOn();
			}

			public override void OnExit(ClearingNode model)
			{
				model.HighlightOff();
			}
		}
	}
}
