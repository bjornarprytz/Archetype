using System;
using Archetype.Godot.UXState;
using Godot;

namespace Archetype.Godot.Targeting
{
    public interface ICanTarget : IClickable
    {
        void HandleTarget(ITargetable target);
    }
}