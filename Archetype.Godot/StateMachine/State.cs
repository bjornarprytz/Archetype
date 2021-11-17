using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Godot;

namespace Archetype.Godot.StateMachine
{
    public interface IState : IDisposable
    {
        void HandleInput(InputEvent inputEvent);
        void Update(float delta);
        IObservable<IStateTransition> OnTransition { get; }

        void Enter();
        void Exit();
    }

    public abstract class State<T> : IState
    {
        protected CompositeDisposable StateActiveLifetime { get; private set; }
        private readonly Subject<IStateTransition> _onTransition = new();
        protected T Model { get; }

        protected State(T model)
        {
            Model = model ?? throw new ArgumentException(nameof(model));
        }

        public IObservable<IStateTransition> OnTransition => _onTransition;

        public void Enter()
        {
            StateActiveLifetime = new CompositeDisposable();
            HandleEnter();
        }

        public virtual void HandleInput(InputEvent inputEvent){}
        public virtual void Update(float delta){}
        protected abstract void HandleEnter();
        protected abstract void HandleExit();
        
        public void Exit()
        {
            HandleExit();
            StateActiveLifetime?.Dispose();
        }

        public void Dispose()
        {
            _onTransition?.Dispose();
            StateActiveLifetime?.Dispose();
        }
        
        protected void TransitionTo<TNextState>()
            where TNextState : IState
        {
            _onTransition.OnNext(Transition<T>.To<TNextState>());
        }
    }
}