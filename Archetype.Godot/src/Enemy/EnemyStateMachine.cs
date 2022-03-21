using Archetype.Godot.StateMachine;
using Godot;

namespace Archetype.Godot.Enemy;

public class EnemyStateMachine : BaseStateMachine<EnemyNode, EnemyStateMachine.Trigger>
{
    private readonly MovingState _moving = new();
    
    public EnemyStateMachine(EnemyNode model) : base(model)
    {
        StateMachine.Configure(Idle)
            .Permit(Trigger.StartMoving, _moving);

        StateMachine.Configure(_moving)
            .Permit(Trigger.StopMoving, Idle);
    }
    
    
    public enum Trigger
    {
        StartMoving,
        StopMoving
    }

    
    private class MovingState : State<EnemyNode>
    {
        public override void OnEnter(EnemyNode model)
        {
            GD.Print($"{model.Name} started moving TODO: Do something here?");
        }

        public override void OnExit(EnemyNode model)
        {
            GD.Print($"{model.Name} stopped moving TODO: Do something here?");
        }
    }
}