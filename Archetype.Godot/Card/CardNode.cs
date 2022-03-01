using System.Reactive.Disposables;
using Godot;
using Archetype.Godot.Targeting;
using Archetype.Prototype1Data;

namespace Archetype.Godot.Card
{
	public class CardNode : Node2D
	{
		private readonly CompositeDisposable _disposables = new ();
		
		
		private CardStateMachine _stateMachine;
		private ICard _cardData;

		public void Construct(ICard cardData)
		{
			_stateMachine = new CardStateMachine(this);
			_cardData = cardData;
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
		
		private void OnInputEvent(object @event)
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



