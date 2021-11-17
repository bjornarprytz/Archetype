using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Archetype.Godot.Card;
using Archetype.Godot.Extensions;
using Godot;

namespace Archetype.Godot.StateMachine
{
	public interface IStateMachine<in T>
	{
		IState<T> CurrentState { get; }
	}
	
	public abstract class StateMachine<T> : Node, IStateMachine<T>
	{
		private readonly T _model;
		private CompositeDisposable _stateLifetime;
		private readonly Dictionary<Type, IState<T>> _states = new();
		private Type _initialState;


		public IState<T> CurrentState { get; private set; }

		protected StateMachine(T model)
		{
			_model = model;
		}
		
		protected void AddState<TState>()
			where TState : IState<T>, new()
		{
			_initialState ??= typeof(TState);

			_states.Add(typeof(TState), new TState());
		}

		public override void _Ready()
		{
			if (_initialState is null)
				throw new Exception("No state to initialise StateMachine with");
				
			foreach (var state in _states.Values)
			{
				state.Init(_model);
				
			}
			
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
		}
	}
}
