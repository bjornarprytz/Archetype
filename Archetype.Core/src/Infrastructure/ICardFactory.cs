using Archetype.Core.Atoms.Cards;
using Archetype.Core.Proto;

namespace Archetype.Core.Infrastructure;

public interface ICardFactory
{
    public ICard CreateCard(IProtoCard protoCard);
}