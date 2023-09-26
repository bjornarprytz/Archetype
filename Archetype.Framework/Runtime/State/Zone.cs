using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public class Zone : Atom, IZone
{
    public override IReadOnlyDictionary<string, CharacteristicInstance> Characteristics { get; }
    public IList<ICard> Cards { get; init; }
}