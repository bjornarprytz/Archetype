using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Archetype.Client;
using Archetype.Godot.Extensions;
using Godot;
using Archetype.Godot.Targeting;
using Archetype.Godot.UXState;

namespace Archetype.Godot.Card
{
	public interface ICardNode : IHoverable, ITargetable, ICanTarget
	{
	}
	
	public class CardNode : Area2D, ICardNode
	{
		private readonly CompositeDisposable _disposables = new ();
		private readonly Subject<bool> _onHovered = new();
		private readonly Subject<InputEventMouseButton> _onClick = new();
		
		private CardStateManager _stateManager;
		private ICardProtoData _cardData;
		private IArchetypeGraphQLClient _client;

		public IObservable<bool> OnHover => _onHovered;
		public IObservable<InputEventMouseButton> OnClick => _onClick;
		
		public Area2D TargetNode => this;

		
		public void Construct(ICardProtoData cardData, IArchetypeGraphQLClient client)
		{
			_stateManager = new CardStateManager(this);
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
			
			
			AddChild(_stateManager);
		}
		
		
		public void HandleTarget(ITargetable target)
		{
			// TODO: Play card here?
		}

		private void OnInputEvent(object viewport, object @event, int shape_idx)
		{
			if (@event is InputEventMouseButton mb)
			{
				_onClick.OnNext(mb);
			}
		}
		
		private void OnMouseEntered()
		{
			_onHovered.OnNext(true);
		}

		private void OnMouseExited()
		{
			_onHovered.OnNext(false);
		}
	}
}
