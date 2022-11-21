using Archetype.Core.Effects;
using Archetype.Core.Effects.Targeting;

namespace Archetype.Core.Proto;

public interface IProtoSpell : IProtoCard
{
    public int Cost { get; }
    
    public IEnumerable<IEffect> Effects { get; } // ordered
    public IEnumerable<ITargetDescriptor> TargetDescriptors { get; } // ordered
}