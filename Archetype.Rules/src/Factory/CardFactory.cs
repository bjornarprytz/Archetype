using Archetype.Core.Atoms.Cards;
using Archetype.Core.Infrastructure;
using Archetype.Core.Proto;
using Archetype.Rules.State;

namespace Archetype.Rules.Factory;

public class CardFactory : ICardFactory
{
    public ICard CreateCard(IProtoCard protoCard)
    {
        return protoCard switch
        {
            IProtoSpell protoSpell => new Spell(protoSpell),
            IProtoUnit protoUnit => new Unit(protoUnit),
            IProtoStructure protoStructure => new Structure(protoStructure),
            _ => throw new InvalidOperationException($"Unknown card type for card [{protoCard.Name}]")
        };
    }
}