using System;

namespace Archetype.Godot.StateMachine
{
    public interface IStateTransition
    {
        Type To { get; }
    }

    public class StateTransition<TTo, TState> : IStateTransition
        where TTo : IState<TState>
    {
        public Type To => typeof(TTo);
    }

    public class StateTransition : IStateTransition
    {
        public StateTransition(Type to)
        {
            To = to;
        }
        public Type To { get; }
    }
    
    public static class Transition<T>
    {
        public static IStateTransition To(Type transitionType)
        {
            return new StateTransition(transitionType);
        }

        public static StateTransition<TTo, T> To<TTo>()
            where TTo : IState<T>
        {
            return new StateTransition<TTo, T>();
        }
    }
}