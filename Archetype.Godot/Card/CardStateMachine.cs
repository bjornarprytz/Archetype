using Archetype.Godot.Extensions;
using Archetype.Godot.StateMachine;
using Archetype.Godot.Targeting;
using Godot;
using Stateless;

namespace Archetype.Godot.Card
{
    public class CardStateMachine : IStateMachine
    {
        private readonly CardNode _model;
        private readonly StateMachine<IState<CardNode>, Triggers> _stateMachine;


        private readonly IdleState _idle = new();
        private readonly HighlightState _highlight = new();
        private readonly TargetingState _targeting = new();
        
        public CardStateMachine(CardNode model)
        {
            _model = model;
            _stateMachine = new StateMachine<IState<CardNode>, Triggers>(_idle);

            _stateMachine.OnTransitioned(state =>
            {
                state.Source.OnExit(_model);
                state.Destination.OnEnter(_model);
            });

            _stateMachine.Configure(_idle)
                .Permit(Triggers.MouseDown, _targeting)
                .Permit(Triggers.HoverStart, _highlight);

            _stateMachine.Configure(_targeting)
                .Permit(Triggers.MouseUp, _idle);
            
            _stateMachine.Configure(_highlight)
                .Permit(Triggers.HoverStop, _idle)
                .SubstateOf(_idle);
        }
        
        public void HandleInput(InputEvent inputEvent)
        {
            _stateMachine.State.HandleInput(_model, inputEvent);

            if (inputEvent is InputEventMouseButton { Pressed: false })
            {
                _stateMachine.FireIfPossible(Triggers.MouseUp);
            }
        }

        public void Process(float delta)
        {
            _stateMachine.State.Process(_model, delta);
        }

        public void MouseEntered()
        {
            _stateMachine.FireIfPossible(Triggers.HoverStart);
        }

        public void MouseExited()
        {
            _stateMachine.FireIfPossible(Triggers.HoverStop);
        }

        public void MouseClick()
        {
            _stateMachine.FireIfPossible(Triggers.MouseDown);
        }
        
        private enum Triggers
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