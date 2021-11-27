using System;
using System.Reactive.Disposables;
using Archetype.Client;
using Archetype.Godot.Extensions;
using Godot;
using Archetype.Godot.Targeting;

namespace Archetype.Godot.Card
{
	public class CardNode : Area2D
	{
		private readonly CompositeDisposable _disposables = new ();
		
		private CardStateMachine _stateMachine;
		private ICardProtoData _cardData;
		private IArchetypeGraphQLClient _client;

		public void Construct(ICardProtoData cardData, IArchetypeGraphQLClient client)
		{
			_stateMachine = new CardStateMachine(this);
			_cardData = cardData;
			_client = client;
			
			_client //TODO: Subscribe to when this card changes instead
				.OnGameStarted
				.Watch()
				.Subscribe((result => GD.Print(result?.Data?.OnGameStarted.Message)))
				.DisposeWith(_disposables);
		}
		
		public override void _Ready()
		{
			base._Ready();
			
			Connect(Signals.CollisionObject2D.InputEvent, this, nameof(OnInputEvent));
			Connect(Signals.CollisionObject2D.MouseEntered, this, nameof(OnMouseEntered));
			Connect(Signals.CollisionObject2D.MouseExited, this, nameof(OnMouseExited));


			var name = GetNode("CardName") as RichTextLabel;
			name.Text = _cardData?.MetaData?.Name;
			var color = GetNode("ColorRect") as ColorRect;
			color.Color = _cardData.MetaData.Color.ToGodot();
			var cost = GetNode("CardCost") as RichTextLabel;
			cost.Text = _cardData.Cost.ToString();
			var text = GetNode("RulesText") as RichTextLabel;
			text.Text = _cardData.RulesText;
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
