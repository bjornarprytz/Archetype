using Archetype.Framework.State;

namespace Archetype.Framework.Design;

public interface IProtoSet
{
    public string Name { get; }
    public string Description { get; }
    public IReadOnlyList<IProtoCard> Cards { get; }
}
