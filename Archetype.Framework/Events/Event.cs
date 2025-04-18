using Archetype.Framework.Effects;

namespace Archetype.Framework.Events;


public record Event
{
    public Guid Id { get; } = Guid.NewGuid();
    
    public required EffectResult Result { get; init; }
}
