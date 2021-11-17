using System;
using System.Reactive.Linq;
using Archetype.Godot.Extensions;
using Archetype.Godot.StateMachine;
using Godot;

namespace Archetype.Godot.UXState
{
    public class HighlightState<T> : State<T>
        where T : Area2D, IHoverable
    {
        protected override void HandleEnter()
        {
            Model.OnHover
                .DistinctUntilChanged()
                .Where(state => !state)
                .Subscribe(_ => TransitionTo<IdleState<T>>())
                .DisposeWith(StateActiveLifetime);
            
            Model.Scale = Vector2.One * 1.1f; 
        }

        protected override void HandleExit()
        {
            Model.Scale = Vector2.One;
        }
    }
    
    public class IdleState<T> : State<T>
        where T : Area2D, IHoverable 
    {
        protected override void HandleEnter()
        {
            Model.OnHover
                .DistinctUntilChanged()
                .Where(state => state)
                .Subscribe(_ => TransitionTo<HighlightState<T>>())
                .DisposeWith(StateActiveLifetime);
        }

        protected override void HandleExit()
        {
            
        }
    }
}