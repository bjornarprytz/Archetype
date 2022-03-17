using Godot;
using Stateless;

namespace Archetype.Godot.StateMachine
{
    public abstract class BaseStateMachine<TModel, TTrigger>
    {
        
        protected StateMachine<IState<TModel>, TTrigger> StateMachine { get; set; }
        protected TModel Model { get; }
    
        protected BaseStateMachine(TModel model)
        {
            Model = model;
        }
        

        public virtual void HandleInput(InputEvent inputEvent)
        {
            StateMachine.State.HandleInput(Model, inputEvent);
        }

        public virtual void Process(float delta)
        {
            StateMachine.State.Process(Model, delta);
        }
    }
}