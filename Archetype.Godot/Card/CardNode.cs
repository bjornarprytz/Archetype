using System.Reactive.Disposables;
using Archetype.Game.Payloads.Pieces;
using Godot;
using Archetype.Godot.Targeting;

namespace Archetype.Godot.Card
{
	public class CardNode : Area2D
	{
		private readonly CompositeDisposable _disposables = new ();
		
		private CardStateMachine _stateMachine;
		private ICard _cardData;
		
		public void Construct(ICard cardData)
		{
			_stateMachine = new CardStateMachine(this);
			_cardData = cardData;

			// TODO: SUbscribe to changes in the card
		}
		
		public override void _Ready()
		{
			base._Ready();
			
			Connect(Signals.CollisionObject2D.InputEvent, this, nameof(OnInputEvent));
			Connect(Signals.CollisionObject2D.MouseEntered, this, nameof(OnMouseEntered));
			Connect(Signals.CollisionObject2D.MouseExited, this, nameof(OnMouseExited));
		}

		public override void _Input(InputEvent @event)
		{
			_stateMachine.HandleInput(@event);
		}

		public override void _Process(float delta)
		{
			_stateMachine.Process(delta);
		}

		public void HandleTarget(ITargetable target)
		{
			// TODO: Play card here?
		}

		private void OnInputEvent(object viewport, object @event, int shape_idx)
		{
			if (@event is InputEventMouseButton { Pressed: true })
			{
				_stateMachine.MouseClick();
			}
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
}
