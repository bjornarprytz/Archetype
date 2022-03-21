using Archetype.Godot.Clearing;
using Archetype.Godot.Extensions;
using Archetype.Godot.StateMachine;
using Godot;
using Stateless;

public class ClearingStateMachine : BaseStateMachine<ClearingNode, ClearingStateMachine.Triggers>
{
    private readonly HighlightState _highlight = new();
    
    public ClearingStateMachine(ClearingNode model) : base(model)
    {
        StateMachine.Configure(Idle)
            .Permit(Triggers.HoverStart, _highlight);

        StateMachine.Configure(_highlight)
            .Permit(Triggers.HoverStop, Idle);
        
        GD.Print("State machine on!");
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
    
    private class HighlightState : State<ClearingNode>
    {
        public override void OnEnter(ClearingNode model)
        { 
            model.HighlightOn();
        }

        public override void OnExit(ClearingNode model)
        {
            model.HighlightOff();
        }
    }

}