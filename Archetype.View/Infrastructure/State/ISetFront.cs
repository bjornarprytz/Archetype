using Archetype.View.Proto;

namespace Archetype.View.Infrastructure.State;

public interface ISetFront
{
    string Name { get; set; }
    IEnumerable<ICardProtoDataFront> Cards { get; }
    IEnumerable<ICreatureProtoDataFront> Creatures { get; }
    IEnumerable<IStructureProtoDataFront> Structures { get; }
}