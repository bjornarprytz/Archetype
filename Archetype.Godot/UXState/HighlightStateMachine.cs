using System;
using System.Reactive.Linq;
using Archetype.Godot.Extensions;
using Archetype.Godot.StateMachine;
using Archetype.Godot.UXState;
using Godot;

namespace Archetype.Godot.Card
{
	public class HighlightStateMachine<T> : StateMachine.StateMachine
		where T : Area2D, IHoverable
	{
		public HighlightStateMachine(T model)
		{
			AddState(new IdleState(model));
			AddState(new HighlightState(model));
		}
		
		private class IdleState : State<T>
		{
			public IdleState(T model) : base(model)
			{
			}
			
			protected override void HandleEnter()
			{
				Model.OnHover
					.DistinctUntilChanged()
					.Where(state => state)
					.Subscribe(_ => TransitionTo<HighlightState>())
					.DisposeWith(StateActiveLifetime);
			}

			protected override void HandleExit()
			{
			
			}

		}

		private class HighlightState : State<T>
		{
			public HighlightState(T model) : base(model) { }
			
			protected override void HandleEnter()
			{
				Model.OnHover
					.DistinctUntilChanged()
					.Where(state => !state)
					.Subscribe(_ => TransitionTo<IdleState>())
					.DisposeWith(StateActiveLifetime);
			
				Model.Scale = Vector2.One * 1.1f; 
			}

			protected override void HandleExit()
			{
				Model.Scale = Vector2.One;
			}

		}
	
			
	}
}
