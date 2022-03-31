using System;
using System.Reactive.Disposables;
using Archetype.Godot.Extensions;
using Archetype.Godot.Infrastructure;
using Godot;
using Archetype.Godot.Targeting;
using Archetype.Prototype1Data;
using Stateless;

namespace Archetype.Godot.Card
{
	public class CardNode : Spatial
	{
		private StateMachine<IState<CardNode>, State.Triggers> _stateMachine;

		private ICard _cardData;

		
		[Inject]
		public void Construct(
			State.Idle idle,
			State.Targeting targeting,
			State.Highlight highlight
		)
		{
			_stateMachine = this.CreateStateMachine(idle, State.ConfigureState(idle, targeting, highlight));
		}
		
		public void Load(ICard cardData)
		{
			_cardData = cardData;

			GetNode<CardViewModel>("Viewport/CardGUI").Load(cardData);
		}

		public override void _Input(InputEvent @event)
		{
			_stateMachine.State.HandleInput(this, @event);

			if (@event is InputEventMouseButton { Pressed: false })
			{
				_stateMachine.FireIfPossible(State.Triggers.MouseUp);
			}
		}

		public override void _Process(float delta)
		{
			_stateMachine.State.Process(this, delta);
		}

		public void HandleTarget(ITargetable target)
		{
			// TODO: Play card here?
		}
		
		private void OnInputEvent(object camera, object @event, Vector3 click_position, Vector3 click_normal, int shape_idx)
		{
			if (@event is not InputEventMouseButton mouseEvent) 
				return;
			
			if (mouseEvent.Pressed)
			{
				_stateMachine.FireIfPossible(State.Triggers.MouseDown);
			}
		}
		
		private void OnMouseEntered()
		{
			_stateMachine.FireIfPossible(State.Triggers.HoverStart);
		}

		private void OnMouseExited()
		{
			_stateMachine.FireIfPossible(State.Triggers.HoverStop);
		}

		public sealed class State
		{
			public static Action<StateMachine<IState<CardNode>, Triggers>> ConfigureState(Idle idle, Targeting targeting, Highlight highlight)
			{
				return 
					sm =>
				{
					sm.Configure(idle)
						.Permit(State.Triggers.MouseDown, targeting)
						.Permit(State.Triggers.HoverStart, highlight);

					sm.Configure(targeting)
						.Permit(State.Triggers.MouseUp, idle);

					sm.Configure(highlight)
						.Permit(State.Triggers.HoverStop, idle)
						.SubstateOf(idle);
				};
			}
			
			public enum Triggers
			{
				HoverStart,
				HoverStop,
				MouseDown,
				MouseUp,
			}

			public class Idle : State<CardNode>
			{
			
			}
			
			public class Highlight : State<CardNode>
            {
            	public override void OnEnter(CardNode model)
            	{
            		model.Scale = Vector3.One*1.1f;
            	}
    
            	public override void OnExit(CardNode model)
            	{
            		model.Scale = Vector3.One;
            	}
            }
            
			public class Targeting : State<CardNode>
            {
	            private readonly ISceneFactory _sceneFactory;
	            private TargetingArrow _targetingArrow;

                public Targeting(ISceneFactory sceneFactory)
                {
	                _sceneFactory = sceneFactory;
                }

                public override void OnEnter(CardNode model)
                {
	                _targetingArrow = _sceneFactory.CreateNode<TargetingArrow>();
            		
            		model.AddChild(_targetingArrow);
            	}
    
            	public override void OnExit(CardNode model)
            	{
            		_targetingArrow.QueueFree();
            	}
            }
		}
		
		
		
	}
}
