namespace Archetype.Framework.Runtime.State;

public class Zone : Atom, IZone
{
    public override IReadOnlyDictionary<string, string> Characteristics { get; }
    public IList<ICard> Cards { get; init; }
}