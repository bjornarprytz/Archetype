using System;
using System.Reactive.Subjects;
using Archetype.Godot.StateMachine;
using Godot;
using Archetype.Godot.Extensions;

namespace Archetype.Godot.Card
{
	public class CardNode : Area2D, ICard
	{
		private StateMachine<ICard> _stateMachine;
		private readonly Subject<bool> _onHovered = new();

		public override void _Ready()
		{
			base._Ready();
			_stateMachine = this.GetRequiredChild<StateMachine<ICard>>();
			_stateMachine.Inject(this);
		}

		public void HighlightOn()
		{
			GD.Print("Highlight ON!");
		}

		public void HighlightOff()
		{
			GD.Print("Highlight OFF!");
		}

		public IObservable<bool> OnHovered => _onHovered;

		public override void _ExitTree()
		{
			_onHovered?.Dispose();
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


