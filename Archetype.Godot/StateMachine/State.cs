using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Godot;

namespace Archetype.Godot.StateMachine
{
    public interface IState<in T> : IDisposable
    {
        void Init(T model);
        
        void HandleInput(InputEvent inputEvent);
        void Update(float delta);
        IObservable<IStateTransition> OnTransition { get; }

        void Enter();
        void Exit();
    }

    public abstract class State<T> : IState<T>
    {
        private bool isInitiated;

        protected CompositeDisposable StateActiveLifetime { get; private set; }
        private readonly Subject<IStateTransition> _onTransition = new();
        protected T Model { get; private set; }

        public void Init(T model)
        {
            if (isInitiated)
                throw new Exception($"Double initialization of State {typeof(T)}");
            
            isInitiated = true;
            
            Model = model ?? throw new ArgumentException(nameof(model));
        }

        public virtual void HandleInput(InputEvent inputEvent)
        {
            
        }

        public virtual void Update(float delta)
        {
        }

        public IObservable<IStateTransition> OnTransition => _onTransition;

        protected void TransitionTo<TNextState>()
            where TNextState : IState<T>
        {
            _onTransition.OnNext(Transition<T>.To<TNextState>());
        }

        public void Enter()
        {
            StateActiveLifetime = new CompositeDisposable();
            HandleEnter();
        }

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
    }
}