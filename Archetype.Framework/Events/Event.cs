using Archetype.Framework.Effects;
using Archetype.Framework.GameLoop;

namespace Archetype.Framework.Events;

public interface IEvent
{
    Guid Id { get; }
    IEffectResult Result { get; }
    IScope Scope { get; }
}

public record Event(IEffectResult Result, IScope Scope) : IEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
