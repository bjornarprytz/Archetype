using System;
using Archetype.Godot.Targeting;
using Archetype.Godot.UXState;

namespace Archetype.Godot.Card
{
    public interface ICard : IHoverable, ITargetable, ICanTarget
    {
        IObservable<IPlayCardContext> OnPlay { get; }
    }

    public interface IPlayCardContext
    {
        // TODO: What to put here? This data should be useful for the Client, in order to bring the required data to bear
    }
}