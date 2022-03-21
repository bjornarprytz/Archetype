using Godot;
using Stateless;

namespace Archetype.Godot.StateMachine
{
	public abstract class BaseStateMachine<TModel, TTrigger>
	{
		protected readonly IdleState Idle = new();
		protected StateMachine<IState<TModel>, TTrigger> StateMachine { get; }
		private TModel Model { get; }
	
		protected BaseStateMachine(TModel model)
		{
			Model = model;
			
			StateMachine = new StateMachine<IState<TModel>, TTrigger>(Idle);

			StateMachine.OnTransitioned(state =>
			{
				state.Source.OnExit(Model);
				state.Destination.OnEnter(Model);
			});
		}
		

		public virtual void HandleInput(InputEvent inputEvent)
		{
			StateMachine.State.HandleInput(Model, inputEvent);
		}

		public virtual void Process(float delta)
		{
			StateMachine.State.Process(Model, delta);
		}
		
		protected class IdleState : State<TModel>
		{
			
		}
	}
}
