using System;
using System.Reactive.Linq;
using Archetype.Godot.Extensions;
using Archetype.Godot.StateMachine;
using Archetype.Godot.Targeting;
using Archetype.Godot.UXState;
using Godot;

namespace Archetype.Godot.Card
{
    public interface ICard : IHighlight, IHoverable, IClickable, IUIAnchor, ITargetable, ICanTarget
    {
        
    }

    public class TargetingState : State<ICard>
    {
        public override void HandleInput(InputEvent inputEvent)
        {
            base.HandleInput(inputEvent);

            switch (inputEvent)
            {
                case InputEventMouseButton { Pressed: false }:
                    var hit = Model.TargetingArrow.GetTarget();
                    TransitionTo<IdleState>();
                    // TODO: Pass target back to the card
                    break;
                case InputEventMouseMotion mm:
                    Model.TargetingArrow.ChangePosition(mm.Position);
                    break;
            }
        }

        protected override void HandleEnter()
        {
            Model.TargetingArrow.Activate();
        }

        protected override void HandleExit()
        {
            Model.TargetingArrow.Deactivate();
        }
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

            Model.OnClick
                .Subscribe(_ => TransitionTo<TargetingState>())
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
            
            Model.OnClick
                .Subscribe(_ => TransitionTo<TargetingState>())
                .DisposeWith(StateActiveLifetime);
        }

        protected override void HandleExit()
        {
            
        }
    }
}