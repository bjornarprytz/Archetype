using System;
using Archetype.Godot.UXState;
using Godot;

namespace Archetype.Godot.Targeting
{
    public interface ICanTarget : IClickable
    {
        ITargetingArrow TargetingArrow { get; }
        void HandleTarget(ITargetable target);
    }
    
    public interface ITargetingArrow
    {
        IObservable<ITargetable> OnTarget { get; }
    }
}