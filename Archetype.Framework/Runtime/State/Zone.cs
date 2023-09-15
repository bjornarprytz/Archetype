namespace Archetype.Framework.Runtime.State;

public class Zone : IZone
{
    public Guid Id { get; init; }
    public IReadOnlyDictionary<string, string> Characteristics { get; init; }
    public IList<ICard> Cards { get; init; }
}