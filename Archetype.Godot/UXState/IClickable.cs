using System;
using System.Reactive;

namespace Archetype.Godot.UXState
{
    public interface IClickable
    {
        IObservable<Unit> OnClick { get; }
    }
}