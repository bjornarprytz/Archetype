using Archetype.Core.Effects;

namespace Archetype.Core.Proto;

public interface IProtoSpell : IProtoPlayingCard
{
    public int Cost { get; }
    
    public IEnumerable<IEffect> Effects { get; } // ordered
    public IEnumerable<ITargetDescriptor> TargetDescriptors { get; } // ordered
}