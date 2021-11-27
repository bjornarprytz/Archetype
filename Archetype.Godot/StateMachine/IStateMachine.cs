using Godot;

namespace Archetype.Godot.StateMachine
{
    public interface IStateMachine
    {
        void HandleInput(InputEvent inputEvent);
        void Process(float delta);
    }
}