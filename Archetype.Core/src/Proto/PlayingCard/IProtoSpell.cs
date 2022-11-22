using Archetype.Core.Effects;
using Archetype.Core.Effects.Resolution;

namespace Archetype.Core.Proto;

public interface IProtoSpell : IProtoPlayingCard, IEffectProvider
{
    public int Cost { get; }
    
    public IEnumerable<ITargetDescriptor> TargetDescriptors { get; } // ordered
}