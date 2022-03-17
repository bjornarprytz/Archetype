using Archetype.Godot.Extensions;
using Archetype.Godot.StateMachine;
using Godot;
using Stateless;

public class ClearingStateMachine : BaseStateMachine<Clearing, ClearingStateMachine.Triggers>
{
    private readonly IdleState _idle = new();
    private readonly HighlightState _highlight = new();
    
    public ClearingStateMachine(Clearing model) : base(model)
    {
        StateMachine = new StateMachine<IState<Clearing>, Triggers>(_idle);

        StateMachine.Configure(_idle)
            .Permit(Triggers.HoverStart, _highlight);

        StateMachine.Configure(_highlight)
            .Permit(Triggers.HoverStop, _idle);
    }
    
    public void MouseEntered()
    {
        StateMachine.FireIfPossible(Triggers.HoverStart);
    }

    public void MouseExited()
    {
        StateMachine.FireIfPossible(Triggers.HoverStop);
    }
    
    public enum Triggers
    {
        HoverStart,
        HoverStop,
    }
    private class IdleState : State<Clearing> 
    { }
    
    private class HighlightState : State<Clearing>
    {
        public override void OnEnter(Clearing model)
        {
            GD.Print("Hover on!");
            model.HighlightOn();
        }

        public override void OnExit(Clearing model)
        {
            GD.Print("Hover off!");
            model.HighlightOff();
        }
    }

}