using Archetype.Godot.StateMachine;

namespace Archetype.Godot.Card
{
	public class CardStateMachine : StateMachine<ICard>
	{
		public override void _Ready()
		{
			base._Ready();
			
			AddState<IdleState>();
			AddState<HighlightState>();
			AddState<TargetingState>();
		}
	}
}
