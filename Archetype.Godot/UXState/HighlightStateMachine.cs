using Archetype.Godot.StateMachine;
using Archetype.Godot.UXState;
using Godot;

namespace Archetype.Godot.Card
{
	public class HighlightStateMachine<T> : StateMachine<T>
		where T : Area2D, IHoverable
	{
		public HighlightStateMachine(T model) : base(model)
		{
			AddState<IdleState<T>>();
			AddState<HighlightState<T>>();
		}
	}
}
