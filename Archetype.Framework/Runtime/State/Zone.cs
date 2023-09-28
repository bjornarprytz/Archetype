using Archetype.Framework.Proto;

namespace Archetype.Framework.Runtime.State;

public class Zone : Atom, IZone
{
    public override IReadOnlyDictionary<string, KeywordInstance> Characteristics { get; }
    public IList<ICard> Cards { get; init; }
}