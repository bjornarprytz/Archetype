using System.Linq;
using System.Reactive.Disposables;
using Archetype.Godot.Extensions;
using Godot;
using Archetype.Godot.Targeting;
using Archetype.Prototype1Data;

namespace Archetype.Godot.Card
{
	public class CardNode : Spatial
	{
		private readonly CompositeDisposable _disposables = new ();
		private readonly CardStateMachine _stateMachine;
		
		private ICard _cardData;

		public CardNode()
		{
			_stateMachine = new CardStateMachine(this);
		}
		
		public void Load(ICard cardData)
		{
			_cardData = cardData;

			GetNode<CardViewModel>("Viewport/CardGUI").Load(cardData);
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
		
		private void OnInputEvent(object camera, object @event, Vector3 click_position, Vector3 click_normal, int shape_idx)
		{
			if (@event is not InputEventMouseButton mouseEvent) 
				return;
			
			if (mouseEvent.Pressed)
			{
				_stateMachine.MouseDown();
			}
			else
			{
				_stateMachine.MouseUp();
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
