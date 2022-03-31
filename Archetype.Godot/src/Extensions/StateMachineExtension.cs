using System;
using Godot;
using Stateless;

namespace Archetype.Godot.Extensions
{
    public static class StateMachineExtension
    {
        public static void FireIfPossible<TState, TTrigger>(this StateMachine<TState, TTrigger> stateMachine, TTrigger trigger)
        {
            if (stateMachine.CanFire(trigger))
            {
                stateMachine.Fire(trigger);
            }
        }
        
        public static StateMachine<IState<TNode>, TTrigger> CreateStateMachine<TNode, TTrigger>(this TNode node, IState<TNode> initialState, Action<StateMachine<IState<TNode>, TTrigger>> configureAction) where TNode : Node
        {
            var stateMachine = new StateMachine<IState<TNode>, TTrigger>(initialState);
            
            configureAction.Invoke(stateMachine);
            
            stateMachine.OnTransitioned(state =>
            {
                state.Source.OnExit(node);
                state.Destination.OnEnter(node);
            });

            return stateMachine;
        }
    }

}