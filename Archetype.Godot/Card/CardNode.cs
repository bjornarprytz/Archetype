using System;
using System.Reactive.Subjects;
using Godot;
using Archetype.Godot.Targeting;

namespace Archetype.Godot.Card
{
	public class CardNode : Area2D, ICard
	{
		private PackedScene _sceneTargetingArrow;
		
		private readonly CardStateManager _stateManager;
		private readonly Subject<bool> _onHovered = new();
		private readonly Subject<InputEventMouseButton> _onClick = new();
		private readonly Subject<IPlayCardContext> _onPlay = new();
		
		public IObservable<bool> OnHover => _onHovered;
		public IObservable<InputEventMouseButton> OnClick => _onClick;
		public IObservable<IPlayCardContext> OnPlay => _onPlay;
		public Area2D TargetNode => this;


		public CardNode()
		{
			_stateManager = new CardStateManager(this);
		}
		
		public override void _Ready()
		{
			base._Ready();
			
			AddChild(_stateManager);
			
			Connect(Signals.CollisionObject2D.InputEvent, this, nameof(OnInputEvent));
			Connect(Signals.CollisionObject2D.MouseEntered, this, nameof(OnMouseEntered));
			Connect(Signals.CollisionObject2D.MouseExited, this, nameof(OnMouseExited));
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
