using System;
using System.Reactive.Subjects;
using Archetype.Client;
using Archetype.Godot.Extensions;
using Godot;
using Archetype.Godot.Targeting;

namespace Archetype.Godot.Card
{
	public class CardNode : Area2D, ICard
	{
		private readonly Subject<bool> _onHovered = new();
		private readonly Subject<InputEventMouseButton> _onClick = new();
		private readonly Subject<IPlayCardContext> _onPlay = new();
		private CardStateManager _stateManager;
		private ICardProtoData _protoData;
		
		public IObservable<bool> OnHover => _onHovered;
		public IObservable<InputEventMouseButton> OnClick => _onClick;
		public IObservable<IPlayCardContext> OnPlay => _onPlay;
		public Area2D TargetNode => this;

		public CardNode()
		{
			_stateManager = new CardStateManager(this);
		}
		
		public void Load(ICardProtoData protoData)
		{
			_protoData = protoData;
		}
		
		public override void _Ready()
		{
			base._Ready();
			
			Connect(Signals.CollisionObject2D.InputEvent, this, nameof(OnInputEvent));
			Connect(Signals.CollisionObject2D.MouseEntered, this, nameof(OnMouseEntered));
			Connect(Signals.CollisionObject2D.MouseExited, this, nameof(OnMouseExited));


			if (_protoData != null)
			{
				var name = GetNode("CardName") as RichTextLabel;
				name.Text = _protoData?.MetaData?.Name;
				var color = GetNode("ColorRect") as ColorRect;
				color.Color = _protoData.MetaData.Color.ToGodot();
				var cost = GetNode("CardCost") as RichTextLabel;
				cost.Text = _protoData.Cost.ToString();
			}
			
			
			AddChild(_stateManager);
			_stateManager.Owner = this;
		}

		public override void _ExitTree()
		{
			_onHovered?.Dispose();
			_onClick?.Dispose();
			_onPlay?.Dispose();
		}
		
		public void HandleTarget(ITargetable target)
		{
			_onPlay.OnNext(null); // TODO: Validate target and send good data instead
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