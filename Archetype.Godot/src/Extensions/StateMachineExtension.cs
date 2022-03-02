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
    }
}