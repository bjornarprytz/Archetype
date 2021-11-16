using System;
using System.Reactive;
using System.Reactive.Subjects;
using Archetype.Godot.StateMachine;
using Godot;
using Archetype.Godot.Extensions;
using Archetype.Godot.Targeting;

namespace Archetype.Godot.Card
{
	public class CardNode : Area2D, ICard
	{
		private IStateMachine<ICard> _stateMachine;
		private readonly Subject<bool> _onHovered = new();
		private readonly Subject<Unit> _onClick = new();
		
		public IObservable<bool> OnHovered => _onHovered;
		public IObservable<Unit> OnClick => _onClick;
		public Vector2 AnchorPosition { get; } = Vector2.Zero;
		public Area2D TargetNode => this;
		public ITargetingArrow TargetingArrow { get; private set; }
		
		public override void _Ready()
		{
			base._Ready();
			_stateMachine = this.GetRequiredChild<StateMachine<ICard>>(); // TODO: Add these as scenes instead
			_stateMachine.Inject(this);
			TargetingArrow = this.GetRequiredChild<TargetingArrow>(); // TODO: Add these as scenes instead
		}

		public void HighlightOn()
		{
			GD.Print("Highlight ON!");
		}

		public void HighlightOff()
		{
			GD.Print("Highlight OFF!");
		}

		

		public override void _ExitTree()
		{
			_onHovered?.Dispose();
		}
		
		private void _on_Area2D_input_event(object viewport, object @event, int shape_idx)
		{
			if (@event is InputEventMouseButton { Pressed: true })
			{
				_onClick.OnNext(Unit.Default);
			}
		}

		private void _on_Area2D_mouse_entered()
		{
			_onHovered.OnNext(true);
		}

		private void _on_Area2D_mouse_exited()
		{
			_onHovered.OnNext(false);
		}

		
		
	}
}