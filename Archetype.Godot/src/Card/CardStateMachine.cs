using Archetype.Godot.Extensions;
using Archetype.Godot.Infrastructure;
using Archetype.Godot.StateMachine;
using Archetype.Godot.Targeting;
using Godot;

namespace Archetype.Godot.Card
{
	public class CardStateMachine : BaseStateMachine<CardNode, CardStateMachine.Triggers>
	{
		private readonly HighlightState _highlight = new();
		private readonly TargetingState _targeting = new();
		
		public CardStateMachine(CardNode model) : base(model)
		{
			StateMachine.Configure(Idle)
				.Permit(Triggers.MouseDown, _targeting)
				.Permit(Triggers.HoverStart, _highlight);

			StateMachine.Configure(_targeting)
				.Permit(Triggers.MouseUp, Idle);
			
			StateMachine.Configure(_highlight)
				.Permit(Triggers.HoverStop, Idle)
				.SubstateOf(Idle);
		}

		public void MouseEntered()
		{
			StateMachine.FireIfPossible(Triggers.HoverStart);
		}

		public void MouseExited()
		{
			StateMachine.FireIfPossible(Triggers.HoverStop);
		}

		public void MouseDown()
		{
			StateMachine.FireIfPossible(Triggers.MouseDown);
		}

		public void MouseUp()
		{
			StateMachine.FireIfPossible(Triggers.MouseUp);
		}
		
		public enum Triggers
		{
			HoverStart,
			HoverStop,
			MouseDown,
			MouseUp,
		}

		private class HighlightState : State<CardNode>
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
		
		private class TargetingState : State<CardNode>
		{
			private TargetingArrow _targetingArrow;

			private ITargetable _currentTarget;
			
			public override void OnEnter(CardNode model)
			{
				_targetingArrow = new TargetingArrow();
				
				model.AddChild(_targetingArrow);
			}

			public override void HandleInput(CardNode model, InputEvent inputEvent)
			{
				switch (inputEvent)
				{
					case InputEventMouseMotion mm:
						_currentTarget = TryTarget(model, mm.Position); 
						break;
				}
			}

			public override void OnExit(CardNode model)
			{
				if (_currentTarget != null)
				{
					model.HandleTarget(_currentTarget);
				}
				
				_targetingArrow.QueueFree();
			}

			private ITargetable TryTarget(CardNode model, Vector2 mousePosition)
			{
				// todo: fix this properly when card is done
				
				return null;
				/*
				 * 
				var spaceState = model.GetWorld().DirectSpaceState;
				var result = spaceState.IntersectRay(mousePosition, collideWithAreas: true);

				if (result == null 
					|| result.Count == 0 
					|| result[0] is not Dictionary d 
					|| d["collider"] is not CollisionShape hit) return null;

				_targetingArrow.PointTo(hit.Transform.origin);

				// TODO: Get the hit object and check if it's targetable
				
				return null;
				 */
			}
		}
	}
}
