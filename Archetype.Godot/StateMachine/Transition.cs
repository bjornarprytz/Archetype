using System;

namespace Archetype.Godot.StateMachine
{
    public interface IStateTransition
    {
        Type To { get; }
    }

    public class StateTransition<TTo> : IStateTransition
        where TTo : IState
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

        public static StateTransition<TTo> To<TTo>()
            where TTo : IState
        {
            return new StateTransition<TTo>();
        }
    }
}