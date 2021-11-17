using System;
using Godot;

namespace Archetype.Godot.UXState
{
    public interface IClickable
    {
        IObservable<InputEventMouseButton> OnClick { get; }
    }
}