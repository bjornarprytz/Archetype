using System;
using Godot;

namespace Archetype.Godot.StateMachine
{
	public interface IState // Marker interface for DI
	{}
	
	public interface IState<in T> : IState
	{
		void HandleInput(T model, InputEvent inputEvent);
		void Process(T model, float delta);

		void OnEnter(T model);
		void OnExit(T model);
	}
	
	public abstract class State<T> : IState<T>
	{
		public virtual void HandleInput(T model, InputEvent inputEvent) {}
		public virtual void Process(T model, float delta) {}
		public virtual void OnEnter(T model) {}
		public virtual void OnExit(T model) {}
	}
}
