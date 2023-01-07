using Archetype.Core.Atoms.Cards;
using Archetype.Core.Proto;

namespace Archetype.Core.Infrastructure;

public interface ICardFactory // TODO: Fullfør denne ideen: En abstract factory for å lage kort fra Proto. Hvordan skal jeg bruke det i spillogikken?
// TODO: NB: Kan hende denne ideen ikke funker
{
    public ICard CreateCard(IProtoCard protoCard);
}