using Archetype.Godot.Extensions;
using Archetype.Godot.StateMachine;
using Archetype.Godot.Targeting;
using Godot;
using Stateless;

namespace Archetype.Godot.Card
{
    public class CardStateMachine : BaseStateMachine<CardNode, CardStateMachine.Triggers>
    {
        private readonly IdleState _idle = new();
        private readonly HighlightState _highlight = new();
        private readonly TargetingState _targeting = new();
        
        public CardStateMachine(CardNode model) : base(model)
        {
            StateMachine = new StateMachine<IState<CardNode>, Triggers>(_idle);

            StateMachine.OnTransitioned(state =>
            {
                state.Source.OnExit(Model);
                state.Destination.OnEnter(Model);
            });

            StateMachine.Configure(_idle)
                .Permit(Triggers.MouseDown, _targeting)
                .Permit(Triggers.HoverStart, _highlight);

            StateMachine.Configure(_targeting)
                .Permit(Triggers.MouseUp, _idle);
            
            StateMachine.Configure(_highlight)
                .Permit(Triggers.HoverStop, _idle)
                .SubstateOf(_idle);
        }
        
        public void MouseEntered()
        {
            StateMachine.FireIfPossible(Triggers.HoverStart);
        }

        public void MouseExited()
        {
            StateMachine.FireIfPossible(Triggers.HoverStop);
        }

        public void MouseDown()
        {
            StateMachine.FireIfPossible(Triggers.MouseDown);
        }

        public void MouseUp()
        {
            StateMachine.FireIfPossible(Triggers.MouseUp);
        }
        
        public enum Triggers
        {
            HoverStart,
            HoverStop,
            MouseDown,
            MouseUp,
        }

        private class IdleState : State<CardNode> { }
        private class HighlightState : State<CardNode>
        {
            public override void OnEnter(CardNode model)
            {
                model.Scale = Vector2.One*1.1f;
            }

            public override void OnExit(CardNode model)
            {
                model.Scale = Vector2.One;
            }
        }
        
        private class TargetingState : State<CardNode>
        {
            private readonly TargetingArrow _targetingArrow;

            public TargetingState()
            {
                _targetingArrow = new TargetingArrow();
            }
            
            public override void OnEnter(CardNode model)
            {
                model.AddChild(_targetingArrow);
            }

            public override void HandleInput(CardNode model, InputEvent inputEvent)
            {
                switch (inputEvent)
                {
                    case InputEventMouseMotion mm:
                        _targetingArrow.PointTo(mm.Position); 
                        break;
                }
            }

            public override void OnExit(CardNode model)
            {
                if (_targetingArrow.TryTarget(out var targetable))
                {
                    model.HandleTarget(targetable);
                }
                
                model.RemoveChild(_targetingArrow);
                _targetingArrow.Reset();
            }
        }
    }
}