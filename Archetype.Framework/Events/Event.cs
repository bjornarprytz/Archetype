using Archetype.Framework.Effects;

namespace Archetype.Framework.Events;


public record Event
{
    public Guid Id { get; } = Guid.NewGuid();
    
    public required IEffectResult Result { get; init; }
}
