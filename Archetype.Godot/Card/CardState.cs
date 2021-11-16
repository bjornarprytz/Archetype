using System;
using System.Reactive;
using System.Reactive.Linq;
using Archetype.Godot.Extensions;
using Archetype.Godot.StateMachine;
using Godot;

namespace Archetype.Godot.Card
{
    public interface ICard
    {
        void HighlightOn();
        void HighlightOff();
        
        IObservable<bool> OnHovered { get; }
    }
    
    public class HighlightState : State<ICard>
    {
        protected override void HandleEnter()
        {
            Model.OnHovered
                .DistinctUntilChanged()
                .Where(state => !state)
                .Subscribe(_ => TransitionTo<IdleState>())
                .DisposeWith(StateActiveLifetime);
            
            Model.HighlightOn();
        }

        protected override void HandleExit()
        {
            Model.HighlightOff();
        }
    }
    
    public class IdleState : State<ICard>
    {
        protected override void HandleEnter()
        {
            Model.OnHovered
                .DistinctUntilChanged()
                .Where(state => state)
                .Subscribe(_ => TransitionTo<HighlightState>())
                .DisposeWith(StateActiveLifetime);
        }

        protected override void HandleExit()
        {
            
        }
    }
}