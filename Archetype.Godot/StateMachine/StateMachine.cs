using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Archetype.Godot.Card;
using Archetype.Godot.Extensions;
using Godot;

namespace Archetype.Godot.StateMachine
{
	public interface IStateMachine
	{
		IState CurrentState { get; }
		IObservable<IState> OnTransition { get; }
	}
	
	public abstract class StateMachine : Node, IStateMachine
	{
		private CompositeDisposable _stateLifetime;
		private readonly Dictionary<Type, IState> _states = new();
		private readonly Subject<IState> _onTransition = new(); 
		private Type _initialState;
		
		public IState CurrentState { get; private set; }
		public IObservable<IState> OnTransition => _onTransition;

		protected void AddState<TState>(TState state)
			where TState : IState
		{
			_initialState ??= typeof(TState);

			_states.Add(typeof(TState), state);
		}

		public override void _Ready()
		{
			if (_initialState is null)
				throw new Exception("No state to initialise StateMachine with");
			
			ChangeState(Transition<ICard>.To(_initialState));
		}

		public override void _Process(float delta)
		{
			CurrentState?.Update(delta);
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			base._UnhandledInput(@event);

			CurrentState?.HandleInput(@event);
		}

		public override void _ExitTree()
		{
			_stateLifetime?.Dispose();
			_onTransition?.Dispose();
		}
		
		private void ChangeState(IStateTransition transition)
		{
			_stateLifetime?.Dispose();
			_stateLifetime = new CompositeDisposable();
			CurrentState?.Exit();
			
			CurrentState = _states[transition.To];
			CurrentState.Enter();

			CurrentState.OnTransition
				.Subscribe(ChangeState)
				.DisposeWith(_stateLifetime);
			
			_onTransition.OnNext(CurrentState);
		}
	}
}
