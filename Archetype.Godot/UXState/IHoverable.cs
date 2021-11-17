using System;

namespace Archetype.Godot.UXState
{
    public interface IHoverable
    {
        IObservable<bool> OnHover { get; }
    }
}