using System;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Archetype.Godot.Extensions;
using Archetype.Godot.StateMachine;
using Archetype.Godot.UXState;
using Godot;
using Godot.Collections;

namespace Archetype.Godot.Targeting
{
    public class TargetingStateMachine<T> : StateMachine.StateMachine
        where T : Node, ICanTarget, IClickable
    {
        public TargetingStateMachine(T model)
        {
            AddState(new IdleState(model));
            AddState(new FindTargetState(model));
        }

        private class IdleState : State<T>
        {
            public IdleState(T model) : base(model)
            {
                
            }
            
            protected override void HandleEnter()
            {
                Model.OnClick
                    .DistinctUntilChanged()
                    .Where(mb => mb.Pressed)
                    .Subscribe(_ => TransitionTo<FindTargetState>())
                    .DisposeWith(StateActiveLifetime);
            }

            protected override void HandleExit() { }
        }
        
        private class FindTargetState : State<T>
        {
            private TargetingArrow _targetingArrow;
            
            public FindTargetState(T model) : base(model)
            {
            }
            
            protected override void HandleEnter()
            {
                _targetingArrow = new TargetingArrow();
                Model.AddChild(_targetingArrow);
            }

            public override void HandleInput(InputEvent inputEvent)
            {
                switch (inputEvent)
                {
                    case InputEventMouseButton { Pressed: false }:
                        TryTarget(_targetingArrow.Points[1]);
                        TransitionTo<IdleState>();
                        break;
                    case InputEventMouseMotion mm:
                        _targetingArrow.PointTo(mm.Position);
                        break;
                }
            }

            protected override void HandleExit()
            {
                _targetingArrow.QueueFree();
            }
            
            private void TryTarget(Vector2 position)
            {
                var spaceState = _targetingArrow.GetWorld2d().DirectSpaceState;
                var result = spaceState.IntersectPoint(position, collideWithAreas: true);

                if (result == null 
                    || result.Count == 0 
                    || result[0] is not Dictionary d 
                    || d["collider"] is not ITargetable targetable) return;
                
                Model.HandleTarget(targetable);
            }
        }
    }
}