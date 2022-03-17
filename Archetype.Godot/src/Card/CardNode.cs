using System.Linq;
using System.Reactive.Disposables;
using Archetype.Godot.Extensions;
using Godot;
using Archetype.Godot.Targeting;
using Archetype.Prototype1Data;

namespace Archetype.Godot.Card
{
	public class CardNode : Node2D
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

			GetNode<Label>("Name").Text = cardData.Name;
			GetNode<Label>("Cost").Text = cardData.Cost.ToString();
			GetNode<Label>("RulesText").Text = cardData.Keywords.IsEmpty() ? "" : cardData.Keywords
				.Select(keyword => keyword.ToString()).Aggregate((keyword, keyword1) => $"{keyword}, {keyword1}");
			
			GetNode<Label>("Defense/Value").Text = cardData.Health.ToString();
			GetNode<Label>("Attack/Value").Text = cardData.Strength.ToString();
			GetNode<Label>("Presence/Value").Text = cardData.Presence.ToString();
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
			if (@event is InputEventMouseButton mouseEvent)
			{
				if (mouseEvent.Pressed)
				{
					_stateMachine.MouseDown();
				}
				else
				{
					_stateMachine.MouseUp();
				}
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



